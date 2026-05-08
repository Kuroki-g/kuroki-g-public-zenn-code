import json
import logging
import os
from pathlib import Path

from mitmproxy import http

from policy import DomainPolicy, get_policy

# ---------------------------------------------------------------------------
# Setup helpers
# ---------------------------------------------------------------------------
def _setup_logger() -> logging.Logger:
    log_path = Path("/logs/access.log")
    log_path.parent.mkdir(parents=True, exist_ok=True)

    fmt = logging.Formatter("%(asctime)s %(message)s", datefmt="%Y-%m-%dT%H:%M:%S")

    file_handler = logging.FileHandler(log_path)
    file_handler.setFormatter(fmt)

    stdout_handler = logging.StreamHandler()
    stdout_handler.setFormatter(fmt)

    log = logging.getLogger("sandbox")
    log.setLevel(logging.INFO)
    log.addHandler(file_handler)
    log.addHandler(stdout_handler)
    return log


def _setup_otel(log: logging.Logger):
    try:
        from opentelemetry import metrics, trace
        from opentelemetry.sdk.resources import Resource
        from opentelemetry.sdk.trace import TracerProvider
        from opentelemetry.sdk.trace.export import BatchSpanProcessor
        from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
        from opentelemetry.sdk.metrics import MeterProvider
        from opentelemetry.sdk.metrics.export import PeriodicExportingMetricReader
        from opentelemetry.exporter.otlp.proto.http.metric_exporter import OTLPMetricExporter

        resource = Resource.create({"service.name": os.environ.get("OTEL_SERVICE_NAME", "sandbox-mitmproxy")})

        provider = TracerProvider(resource=resource)
        # OTLPSpanExporter/OTLPMetricExporter read OTEL_EXPORTER_OTLP_ENDPOINT and append /v1/traces, /v1/metrics
        provider.add_span_processor(BatchSpanProcessor(OTLPSpanExporter()))
        trace.set_tracer_provider(provider)
        tracer = trace.get_tracer(__name__)

        meter_provider = MeterProvider(
            resource=resource,
            metric_readers=[PeriodicExportingMetricReader(OTLPMetricExporter())],
        )
        metrics.set_meter_provider(meter_provider)
        request_counter = metrics.get_meter(__name__).create_counter(
            "sandbox.requests",
            description="Requests processed by sandbox proxy",
        )

        log.info("OTel initialised → %s", os.environ.get("OTEL_EXPORTER_OTLP_ENDPOINT", "(no endpoint set)"))
        return tracer, request_counter
    except Exception as exc:
        log.warning("OTel init skipped: %s", exc)
        return None, None


logger = _setup_logger()
_tracer, _request_counter = _setup_otel(logger)

# flow.metadata keys
_SPAN_KEY = "_otel_span"
_VERDICT_KEY = "_sandbox_verdict"
_REASON_KEY = "_sandbox_block_reason"


# ---------------------------------------------------------------------------
# Addon
# ---------------------------------------------------------------------------
class SandboxFilter:
    # -- mitmproxy hooks --

    def request(self, flow: http.HTTPFlow) -> None:
        self._start_span(flow)
        policy = get_policy(flow.request.pretty_host)
        if policy is None:
            host = flow.request.pretty_host
            logger.warning("BLOCK domain  %s %s", flow.request.method, flow.request.pretty_url)
            guidance = (
                f"Domain '{host}' is not whitelisted. "
                "Before proposing to add it, self-check: "
                "(1) Is this domain directly required for the current research task? "
                "(2) Is it a legitimate public domain (not a personal/private service)? "
                "(3) Does it fit the research scope (weather data, building standards, academic papers)? "
                "If all checks pass, ask the user for permission to add it to .sandbox/proxy/whitelist.toml "
                "before making any changes. "
                "If any check fails, report why you attempted this access instead."
            )
            body = self._block_body("domain_not_whitelisted", guidance, domain=host)
            self._set_blocked(flow, "domain", http.Response.make(403, body, {"Content-Type": "application/json"}))
            return
        for check in (self._method_filter, self._dlp_filter):
            if check(flow, policy):
                return  # response set on flow → response() hook will close the span

    def response(self, flow: http.HTTPFlow) -> None:
        verdict = flow.metadata.get(_VERDICT_KEY, "allow")
        if flow.response and verdict == "allow":
            logger.info("ALLOW %s %s %s", flow.request.method, flow.response.status_code, flow.request.pretty_url)
        self._close_span(
            flow,
            verdict=verdict,
            block_reason=flow.metadata.get(_REASON_KEY, ""),
            status_code=flow.response.status_code if flow.response else 0,
        )

    # -- filters --

    def _method_filter(self, flow: http.HTTPFlow, policy: DomainPolicy) -> bool:
        if flow.request.method not in policy.methods:
            logger.warning("BLOCK method  %s %s", flow.request.method, flow.request.pretty_url)
            body = self._block_body(
                "method_not_allowed",
                f"The sandbox proxy blocked this request. '{flow.request.method}' is not permitted; use one of {sorted(policy.methods)}.",
                method=flow.request.method,
                allowed_methods=sorted(policy.methods),
            )
            self._set_blocked(flow, "method", http.Response.make(405, body, {"Content-Type": "application/json"}))
            return True
        return False

    def _dlp_filter(self, flow: http.HTTPFlow, policy: DomainPolicy) -> bool:
        body = flow.request.content.decode("utf-8", errors="ignore") if flow.request.content else ""
        hit = policy.dlp(flow.request.pretty_url, body)
        if hit:
            label, snippet = hit
            logger.warning("BLOCK dlp     %s %s [%s: %s]", flow.request.method, flow.request.pretty_url, label, snippet)
            body = self._block_body(
                "dlp_policy_violation",
                f"The sandbox proxy blocked this request. A secret of type '{label}' was detected in the request. Remove credentials before sending.",
                secret_type=label,
            )
            self._set_blocked(flow, "dlp", http.Response.make(400, body, {"Content-Type": "application/json"}))
            return True
        return False

    # -- private helpers --

    def _start_span(self, flow: http.HTTPFlow) -> None:
        if _tracer is None:
            return
        flow.metadata[_SPAN_KEY] = _tracer.start_span(
            f"{flow.request.method} {flow.request.pretty_host}",
            attributes={
                "http.method": flow.request.method,
                "http.url": flow.request.pretty_url,
                "net.peer.name": flow.request.pretty_host,
            },
        )

    def _set_blocked(self, flow: http.HTTPFlow, reason: str, response: http.Response | None = None) -> None:
        flow.metadata[_VERDICT_KEY] = "block"
        flow.metadata[_REASON_KEY] = reason
        if response is not None:
            flow.response = response

    def _block_body(self, reason: str, message: str, **extras) -> bytes:
        return json.dumps({"blocked_by": "sandbox-proxy", "reason": reason, "message": message, **extras}).encode()

    def _close_span(
        self,
        flow: http.HTTPFlow,
        verdict: str = "block",
        block_reason: str = "",
        status_code: int = 0,
    ) -> None:
        span = flow.metadata.pop(_SPAN_KEY, None)
        if span is None:
            return
        span.set_attribute("sandbox.verdict", verdict)
        if block_reason:
            span.set_attribute("sandbox.block_reason", block_reason)
        if status_code:
            span.set_attribute("http.status_code", status_code)
        span.end()

        if _request_counter is not None:
            attrs = {"sandbox.verdict": verdict, "net.peer.name": flow.request.pretty_host}
            if block_reason:
                attrs["sandbox.block_reason"] = block_reason
            _request_counter.add(1, attrs)


addons = [SandboxFilter()]
