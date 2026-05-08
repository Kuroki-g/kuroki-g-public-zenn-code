#!/bin/sh
set -e

# Copy our custom CA cert into the writable confdir on first start.
# mitmproxy uses this as its signing CA and writes per-domain certs alongside it.
if [ ! -f /data/mitmproxy-ca.pem ]; then
    echo "[entrypoint] Installing custom CA certificate into confdir..."
    cp /certs/mitmproxy-ca.pem /data/mitmproxy-ca.pem
fi

exec "$@"
