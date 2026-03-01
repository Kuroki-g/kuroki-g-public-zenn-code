# syntax=docker/dockerfile:1
# check=error=true
FROM python:3.14-slim

ARG USER=vscode
ARG UID=1000
ARG GID=1000

RUN apt-get update \
  && apt-get install -y --no-install-recommends \
  bash-completion \
  curl \
  git \
  gnupg \
  iproute2 \
  socat \
  && rm -rf /var/lib/apt/lists/*

# --- Create non root user ---
RUN groupadd --gid ${GID} ${USER} \
  && useradd \
  --uid ${UID} \
  --gid ${GID} \
  --shell /bin/bash \
  --create-home \
  ${USER}
RUN mkdir -p /workspace && chown ${UID}:${GID} /workspace

USER ${USER}
RUN curl -fsSL https://claude.ai/install.sh | bash

WORKDIR /workspace

CMD ["bash"]
