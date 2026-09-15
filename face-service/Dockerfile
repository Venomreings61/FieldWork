# FieldWork Face Service — Phase 5.2.6
#
# Model weights are downloaded and SHA-256 verified during this build, not
# committed to git and not downloaded at container startup. If the pinned
# checksums ever fail to match (upstream file changed, network issue serving
# corrupted bytes, etc.), this build fails loudly rather than producing an
# image running against unverified/unexpected model weights.

FROM python:3.12-slim AS base

# opencv-python-headless still needs a couple of shared libs at runtime
# even without GUI support.
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgl1 \
    libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY requirements.txt .
# Strip the test-only dependency comment lines; only install what's actually
# listed as active (uncommented) in requirements.txt.
RUN pip install --no-cache-dir -r requirements.txt

# --- Model download + verification stage ---
ENV UNIFACE_CACHE_DIR=/root/.uniface/models
COPY download_models.py .
RUN python3 download_models.py

# --- Application code ---
COPY app ./app

EXPOSE 8500

# Basic container-level healthcheck hitting the liveness endpoint — this is
# deliberately /health/live, not /health/ready: Docker's healthcheck should
# reflect "is the process alive," matching the same live/ready split the
# app itself exposes for a real orchestrator (ECS, k8s) to distinguish.
HEALTHCHECK --interval=10s --timeout=3s --start-period=15s --retries=3 \
    CMD python3 -c "import urllib.request; urllib.request.urlopen('http://localhost:8500/health/live', timeout=2)" || exit 1

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8500"]
