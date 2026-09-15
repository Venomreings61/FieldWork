"""
Phase 5.2.2 — Minimal SCRFD 10G + ArcFace ResNet inference POC.

Pinned models (see Phase 5.2.1):
  Detection:   scrfd_10g_kps.onnx   sha256=5838f7fe053675b1c7a08b633df49e7af5495cee0493c7dcf6697200b85b5b91
  Recognition: w600k_r50.onnx       sha256=4c06341c33c2ca1f86781dab0e829f88ad5b64be9fba56e56bc9ebdefc619e43

Usage:
  python3 test_face.py <image_a> <image_b>

Behavior:
  - Detects exactly one face in each image (SCRFD + 5-point landmarks).
  - Zero faces or multiple faces -> controlled failure, no embedding computed.
  - On success, produces a 512-d normalized embedding per image via ArcFace ResNet.
  - Prints cosine similarity between the two embeddings.

This script is intentionally independent of FieldWork's API/DB — it only
proves the model pipeline works correctly before any service wrapper is built.
"""

import sys

from uniface import SCRFD, ArcFace, compute_similarity
from uniface.constants import ArcFaceWeights, SCRFDWeights
import cv2


class FaceDetectionError(Exception):
    """Raised when an image does not contain exactly one detectable face."""


def get_single_face_embedding(detector: SCRFD, recognizer: ArcFace, image_path: str):
    image = cv2.imread(image_path)
    if image is None:
        raise FaceDetectionError(f"Could not read image file: {image_path}")

    faces = detector.detect(image)

    if len(faces) == 0:
        raise FaceDetectionError(f"No face detected in: {image_path}")
    if len(faces) > 1:
        raise FaceDetectionError(
            f"Multiple faces ({len(faces)}) detected in: {image_path}. "
            "Exactly one face is required for enrollment/verification."
        )

    face = faces[0]
    embedding = recognizer.get_normalized_embedding(image, face.landmarks)
    return embedding


def main():
    if len(sys.argv) != 3:
        print("Usage: python3 test_face.py <image_a> <image_b>")
        sys.exit(1)

    image_a_path, image_b_path = sys.argv[1], sys.argv[2]

    print("Loading models (SCRFD 10G + ArcFace ResNet)...")
    detector = SCRFD(model_name=SCRFDWeights.SCRFD_10G_KPS)
    recognizer = ArcFace(model_name=ArcFaceWeights.RESNET)
    print("Models loaded.\n")

    try:
        print(f"Processing {image_a_path} ...")
        embedding_a = get_single_face_embedding(detector, recognizer, image_a_path)
        print(f"  -> embedding shape: {embedding_a.shape}")

        print(f"Processing {image_b_path} ...")
        embedding_b = get_single_face_embedding(detector, recognizer, image_b_path)
        print(f"  -> embedding shape: {embedding_b.shape}")

    except FaceDetectionError as e:
        print(f"\nCONTROLLED FAILURE: {e}")
        sys.exit(2)

    similarity = compute_similarity(embedding_a, embedding_b, normalized=True)
    print(f"\nCosine similarity: {similarity:.4f}")


if __name__ == "__main__":
    main()
