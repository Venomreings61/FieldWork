"""
Loads the pinned SCRFD + ArcFace models and exposes face detection/embedding.

Readiness is tracked explicitly and separately from process liveness (see
main.py's /health/live vs /health/ready split) — this module's state is what
/health/ready actually reports on.
"""

import hashlib
import logging
import os
import threading

import cv2
import numpy as np
from uniface import SCRFD, ArcFace, compute_similarity
from uniface.constants import ArcFaceWeights, SCRFDWeights

from app.config import ARCFACE_RESNET, SCRFD_10G, UNIFACE_CACHE_DIR, ModelArtifact

logger = logging.getLogger("face_engine")


class ModelIntegrityError(Exception):
    """Raised when a model file on disk does not match its pinned SHA-256 hash."""


class FaceDetectionError(Exception):
    """Raised when an image does not contain exactly one detectable face."""


def _verify_checksum(path: str, artifact: ModelArtifact) -> None:
    if not os.path.exists(path):
        raise ModelIntegrityError(f"Expected model file not found: {path}")

    sha256 = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(8192), b""):
            sha256.update(chunk)

    actual = sha256.hexdigest()
    if actual != artifact.sha256:
        raise ModelIntegrityError(
            f"Checksum mismatch for {artifact.filename}: "
            f"expected {artifact.sha256}, got {actual}. "
            "Refusing to load a model that does not match the pinned artifact."
        )


class FaceEngine:
    """
    Thread-safe holder for the loaded SCRFD detector and ArcFace recognizer.
    """

    def __init__(self) -> None:
        self._detector: SCRFD | None = None
        self._recognizer: ArcFace | None = None
        self._lock = threading.Lock()
        self._ready = False
        self._load_error: str | None = None

    @property
    def is_ready(self) -> bool:
        return self._ready

    @property
    def load_error(self) -> str | None:
        return self._load_error

    def load(self) -> None:
        with self._lock:
            try:
                scrfd_path = os.path.join(UNIFACE_CACHE_DIR, SCRFD_10G.filename)
                arcface_path = os.path.join(UNIFACE_CACHE_DIR, ARCFACE_RESNET.filename)

                logger.info("Verifying pinned model checksums...")
                _verify_checksum(scrfd_path, SCRFD_10G)
                _verify_checksum(arcface_path, ARCFACE_RESNET)
                logger.info("Checksums verified for both models.")

                logger.info("Loading SCRFD 10G detector...")
                self._detector = SCRFD(model_name=SCRFDWeights.SCRFD_10G_KPS)

                logger.info("Loading ArcFace ResNet recognizer...")
                self._recognizer = ArcFace(model_name=ArcFaceWeights.RESNET)

                self._ready = True
                self._load_error = None
                logger.info("Face engine ready.")

            except Exception as exc:
                self._ready = False
                self._load_error = str(exc)
                logger.exception("Face engine failed to load.")
                raise

    def get_single_face_embedding(self, image_bytes: bytes) -> np.ndarray:
        if not self._ready:
            raise RuntimeError("Face engine is not ready.")

        image_array = np.frombuffer(image_bytes, dtype=np.uint8)
        image = cv2.imdecode(image_array, cv2.IMREAD_COLOR)
        if image is None:
            raise FaceDetectionError("Could not decode image data.")

        faces = self._detector.detect(image)

        if len(faces) == 0:
            raise FaceDetectionError("No face detected in the provided image.")
        if len(faces) > 1:
            raise FaceDetectionError(
                f"Multiple faces ({len(faces)}) detected. "
                "Exactly one face is required."
            )

        face = faces[0]
        return self._recognizer.get_normalized_embedding(image, face.landmarks)

    @staticmethod
    def similarity(embedding_a: np.ndarray, embedding_b: np.ndarray) -> float:
        return float(compute_similarity(embedding_a, embedding_b, normalized=True))


engine = FaceEngine()