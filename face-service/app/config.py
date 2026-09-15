import os
from dataclasses import dataclass


@dataclass
class ModelArtifact:
    filename: str
    sha256: str


# Artifact definitions pinned with actual SHA-256 checksums
SCRFD_10G = ModelArtifact(
    filename="scrfd_10g.onnx",
    sha256="5838f7fe053675b1c7a08b633df49e7af5495cee0493c7dcf6697200b85b5b91",
)

ARCFACE_RESNET = ModelArtifact(
    filename="arcface_resnet.onnx",
    sha256="4c06341c33c2ca1f86781dab0e829f88ad5b64be9fba56e56bc9ebdefc619e43",
)

UNIFACE_CACHE_DIR = os.environ.get(
    "UNIFACE_CACHE_DIR", os.path.expanduser("~/.uniface/models")
)

# Application constants
EMBEDDING_DIM = 512
DEFAULT_VERIFICATION_THRESHOLD = 0.40