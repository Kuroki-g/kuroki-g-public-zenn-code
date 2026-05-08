import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent))

from policy import (
    DomainPolicy,
    _dlp_scan,
    default_dlp,
    get_policy,
    no_dlp,
)


# ---------------------------------------------------------------------------
# _dlp_scan
# ---------------------------------------------------------------------------
class TestDlpScan(unittest.TestCase):
    def test_anthropic_key(self):
        key = "sk-ant-" + "x" * 30
        result = _dlp_scan(f"Authorization: Bearer {key}")
        assert result is not None
        label, snippet = result
        assert label == "anthropic_api_key"
        assert snippet.endswith("[redacted]")

    def test_openai_key(self):
        key = "sk-" + "a" * 48
        result = _dlp_scan(key)
        assert result is not None
        assert result[0] == "openai_api_key"

    def test_github_token(self):
        # gh[pousr]_: p=personal, o=oauth, u=app user, s=app installation, r=refresh
        for prefix in ("ghp_", "gho_", "ghu_", "ghs_", "ghr_"):
            token = prefix + "a" * 36
            result = _dlp_scan(token)
            assert result is not None, f"should detect {prefix} token"
            assert result[0] == "github_token"

    def test_aws_access_key(self):
        key = "AKIA" + "A" * 16
        result = _dlp_scan(key)
        assert result is not None
        assert result[0] == "aws_access_key_id"

    def test_gcp_api_key(self):
        key = "AIza" + "a" * 35
        result = _dlp_scan(key)
        assert result is not None
        assert result[0] == "gcp_api_key"

    def test_slack_token(self):
        for prefix in ("xoxb-", "xoxa-", "xoxp-", "xoxr-", "xoxs-"):
            token = prefix + "1234-5678"
            result = _dlp_scan(token)
            assert result is not None, f"should detect {prefix} token"
            assert result[0] == "slack_token"

    def test_private_key_header(self):
        pem = "-----BEGIN " + "RSA PRIVATE KEY-----"
        result = _dlp_scan(pem)
        assert result is not None
        assert result[0] == "private_key"

    def test_jwt(self):
        # eyJ... . eyJ... . <sig>  (base64url segments)
        jwt = "eyJ" "hbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1c2VyIn0.abc123"
        result = _dlp_scan(jwt)
        assert result is not None
        assert result[0] == "jwt"

    def test_clean_text(self):
        assert _dlp_scan("hello world, no secrets here") is None

    def test_redact_truncates(self):
        key = "sk-ant-" + "x" * 30
        _, snippet = _dlp_scan(key)
        assert "[redacted]" in snippet
        assert len(snippet) < len(key)


# ---------------------------------------------------------------------------
# default_dlp / no_dlp
# ---------------------------------------------------------------------------
class TestDlpFunctions(unittest.TestCase):
    def test_default_dlp_detects_in_url(self):
        key = "sk-ant-" + "x" * 30
        result = default_dlp(f"https://api.anthropic.com/v1/messages?key={key}", "")
        assert result is not None
        assert result[0] == "anthropic_api_key"

    def test_default_dlp_detects_in_body(self):
        key = "sk-ant-" + "x" * 30
        result = default_dlp("https://api.anthropic.com/v1/messages", f'{{"api_key": "{key}"}}')
        assert result is not None
        assert result[0] == "anthropic_api_key"

    def test_default_dlp_clean(self):
        assert default_dlp("https://api.anthropic.com/v1/messages", '{"model": "claude-3"}') is None

    def test_no_dlp_always_passes(self):
        key = "sk-ant-" + "x" * 30
        assert no_dlp(f"https://example.com?key={key}", key) is None


# ---------------------------------------------------------------------------
# get_policy
# ---------------------------------------------------------------------------
class TestGetPolicy(unittest.TestCase):
    def test_exact_domain(self):
        assert get_policy("anthropic.com") is not None

    def test_subdomain(self):
        assert get_policy("api.anthropic.com") is not None

    def test_unknown_domain(self):
        assert get_policy("evil.com") is None

    def test_partial_match_not_allowed(self):
        # "notanthopic.com" should not match "anthropic.com"
        assert get_policy("notanthropic.com") is None

    def test_anthropic_allows_post(self):
        policy = get_policy("anthropic.com")
        assert "POST" in policy.methods

    def test_github_default_methods(self):
        policy = get_policy("github.com")
        assert policy.methods == frozenset({"GET", "HEAD"})

    def test_subdomain_inherits_policy(self):
        policy = get_policy("docs.github.com")
        assert policy is not None
        assert "POST" not in policy.methods


# ---------------------------------------------------------------------------
# DomainPolicy defaults
# ---------------------------------------------------------------------------
class TestDomainPolicy(unittest.TestCase):
    def test_default_methods(self):
        p = DomainPolicy()
        assert p.methods == frozenset({"GET", "HEAD"})

    def test_default_dlp_is_default_dlp(self):
        p = DomainPolicy()
        assert p.dlp is default_dlp

    def test_custom_dlp(self):
        p = DomainPolicy(dlp=no_dlp)
        assert p.dlp is no_dlp


if __name__ == "__main__":
    unittest.main()
