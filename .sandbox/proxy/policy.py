import re
import sys
from dataclasses import dataclass, field
from pathlib import Path
from typing import Callable

if sys.version_info >= (3, 11):
    import tomllib
else:
    import tomli as tomllib

# ---------------------------------------------------------------------------
# DLP – secret patterns
# ---------------------------------------------------------------------------
_DLP_PATTERNS: list[tuple[str, re.Pattern[str]]] = [
    ("anthropic_api_key",   re.compile(r"sk-" r"ant-[a-zA-Z0-9\-_]{20,}")),
    ("openai_api_key",      re.compile(r"sk" r"-[a-zA-Z0-9]{48}")),
    ("github_token",        re.compile(r"gh[pousr]" r"_[a-zA-Z0-9]{36,255}")),
    ("github_pat",          re.compile(r"github_pat" r"_[a-zA-Z0-9_]{82}")),
    ("aws_access_key_id",   re.compile(r"AK" r"IA[0-9A-Z]{16}")),
    ("aws_secret_key",      re.compile(r"(?i)aws.{0,20}" r"secret.{0,20}['\"][0-9a-zA-Z/+]{40}['\"]")),
    ("gcp_api_key",         re.compile(r"AI" r"za[0-9A-Za-z\-_]{35}")),
    ("slack_token",         re.compile(r"xox" r"[baprs]-[0-9A-Za-z\-]+")),
    ("private_key",         re.compile(r"-----" r"BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY" r"-----")),
    ("jwt",                 re.compile(r"eyJ" r"[a-zA-Z0-9_-]+\.eyJ[a-zA-Z0-9_-]+\.[a-zA-Z0-9_-]+")),
]


def _redact(m: re.Match[str]) -> str:
    return m.group()[:6] + "[redacted]"


def _dlp_scan(text: str) -> tuple[str, str] | None:
    """Return (label, redacted_snippet) for the first pattern hit, else None."""
    for label, pattern in _DLP_PATTERNS:
        m = pattern.search(text)
        if m:
            return label, _redact(m)
    return None


# ---------------------------------------------------------------------------
# DLP functions  DlpFn = (url, body) -> (label, snippet) | None
# ---------------------------------------------------------------------------
DlpFn = Callable[[str, str], tuple[str, str] | None]


def default_dlp(url: str, body: str) -> tuple[str, str] | None:
    """Scan URL and request body for known secret patterns."""
    for text in (url, body):
        hit = _dlp_scan(text)
        if hit:
            return hit
    return None


def no_dlp(url: str, body: str) -> tuple[str, str] | None:
    """Disable DLP for this domain."""
    return None


# ---------------------------------------------------------------------------
# Per-domain policy
# ---------------------------------------------------------------------------
@dataclass
class DomainPolicy:
    methods: frozenset[str] = frozenset({"GET", "HEAD"})
    dlp: DlpFn = field(default=default_dlp)


_DLP_FNS: dict[str, DlpFn] = {
    "default": default_dlp,
    "none":    no_dlp,
}

_WHITELIST_PATH = Path(__file__).with_name("whitelist.toml")

def _load_policies() -> dict[str, DomainPolicy]:
    with open(_WHITELIST_PATH, "rb") as f:
        data = tomllib.load(f)
    policies: dict[str, DomainPolicy] = {}
    for host, entry in data.get("domains", {}).items():
        methods = frozenset(entry.get("methods", ["GET", "HEAD"]))
        dlp_key = entry.get("dlp", "default")
        dlp_fn  = _DLP_FNS.get(dlp_key, default_dlp)
        policies[host] = DomainPolicy(methods=methods, dlp=dlp_fn)
    return policies

POLICIES: dict[str, DomainPolicy] = _load_policies()


def get_policy(host: str) -> DomainPolicy | None:
    for domain, policy in POLICIES.items():
        if host == domain or host.endswith("." + domain):
            return policy
    return None
