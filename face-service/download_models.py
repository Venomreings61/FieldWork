"""
Downloads and verifies the exact pinned model artifacts.

Run at Docker build time, NOT at container startup — this keeps the ~182MB
of model weights out of git while still guaranteeing the running container
has exactly the artifacts this project was validated against.

Deliberately self-contained (no import from app.config) — this script runs
BEFORE `COPY app ./app` in the Dockerfile, so the app package does not exist
yet at the point this executes.

The build FAILS if either checksum doesn't match. That's intentional.
"""

import hashlib
import os
import sys
import urllib.request

MODELS = [
    {
        "name": "scrfd_10g",
        "url": "https://github.com/yakhyo/uniface/releases/download/weights/scrfd_10g_kps.onnx",
        "sha256": "5838f7fe053675b1c7a08b633df49e7af5495cee0493c7dcf6697200b85b5b91",
        "filename": "scrfd_10g.onnx",
    },
    {
        "name": "arcface_resnet",
        "url": "https://github.com/yakhyo/uniface/releases/download/weights/w600k_r50.onnx",
        "sha256": "4c06341c33c2ca1f86781dab0e829f88ad5b64be9fba56e56bc9ebdefc619e43",
        "filename": "arcface_resnet.onnx",
    },
]


def sha256_of(path: str) -> str:
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(8192), b""):
            h.update(chunk)
    return h.hexdigest()


def main() -> None:
    target_dir = os.environ.get("UNIFACE_CACHE_DIR", "/root/.uniface/models")
    os.makedirs(target_dir, exist_ok=True)

    for model in MODELS:
        dest = os.path.join(target_dir, model["filename"])
        print(f"Downloading {model['name']} to {dest} ...", flush=True)
        urllib.request.urlretrieve(model["url"], dest)

        print(f"Verifying SHA-256 for {model['name']} ...", flush=True)
        actual = sha256_of(dest)
        if actual != model["sha256"]:
            print(
                f"CHECKSUM MISMATCH for {model['name']}: "
                f"expected {model['sha256']}, got {actual}. Aborting build.",
                file=sys.stderr,
            )
            sys.exit(1)

        print(f"OK: {model['name']} verified ({actual}).", flush=True)

    print("All model artifacts downloaded and verified.", flush=True)


if __name__ == "__main__":
    main()