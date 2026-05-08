FROM mitmproxy/mitmproxy:latest

RUN pip install --no-cache-dir \
    opentelemetry-sdk \
    opentelemetry-exporter-otlp-proto-http
