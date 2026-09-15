"""
FieldWork Face Service — Phase 5.2.3.

Currently exposes only health endpoints, split to mirror FieldWork.Api's own
liveness/readiness pattern:

  GET /health/live   — process is running. No model checks. Always 200 once
                        the ASGI server has started, regardless of model state.
  GET /health/ready  — SCRFD + ArcFace are loaded and verified. 503 if model
                        loading failed or hasn't completed yet.

Model loading happens once, synchronously, at startup — not lazily on first
request — so /health/ready reflects true readiness rather than "the process
exists but nobody's tried inference yet."
"""

import json
import logging

import numpy as np
from fastapi import FastAPI, File, Form, UploadFile
from fastapi.responses import JSONResponse

from app.config import ARCFACE_RESNET, DEFAULT_VERIFICATION_THRESHOLD, EMBEDDING_DIM, SCRFD_10G
from app.face_engine import FaceDetectionError, engine
from app.schemas import EmbeddingResponse, ErrorResponse, VerifyResponse

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("face_service")

app = FastAPI(title="FieldWork Face Service", version="0.1.0")


@app.on_event("startup")
def startup_load_models() -> None:
    logger.info("Startup: loading face models...")
    try:
        engine.load()
    except Exception:
        # Deliberately do not crash the process on model load failure.
        # /health/ready will correctly report 503, which is the signal
        # an orchestrator (Docker healthcheck, ECS, etc.) should act on,
        # rather than a crash-loop that gives no diagnostic information.
        logger.error("Model loading failed at startup. Service will report not-ready.")


@app.get("/health/live")
def health_live():
    return {"status": "alive"}


@app.get("/health/ready")
def health_ready():
    if engine.is_ready:
        return {
            "status": "ready",
            "detector": SCRFD_10G.filename,
            "detector_sha256": SCRFD_10G.sha256,
            "recognizer": ARCFACE_RESNET.filename,
            "recognizer_sha256": ARCFACE_RESNET.sha256,
        }

    return JSONResponse(
        status_code=503,
        content={
            "status": "not_ready",
            "error": engine.load_error or "Models have not finished loading.",
        },
    )


@app.post(
    "/v1/face/embedding",
    response_model=EmbeddingResponse,
    responses={
        422: {"model": ErrorResponse},
        503: {"model": ErrorResponse},
    },
)
async def create_embedding(image: UploadFile = File(...)):
    if not engine.is_ready:
        return JSONResponse(
            status_code=503,
            content=ErrorResponse(
                error_code="SERVICE_NOT_READY",
                detail="Face models are not loaded. Check /health/ready.",
            ).model_dump(),
        )

    image_bytes = await image.read()

    try:
        embedding = engine.get_single_face_embedding(image_bytes)
    except FaceDetectionError as exc:
        # Distinguish the three distinct failure modes for the caller —
        # a client (or FieldWork.Api) may want to handle each differently
        # ("please retake", "ensure one person in frame", "unreadable file").
        message = str(exc)
        if "Could not decode" in message:
            error_code = "INVALID_IMAGE"
        elif "Multiple faces" in message:
            error_code = "MULTIPLE_FACES_DETECTED"
        else:
            error_code = "NO_FACE_DETECTED"

        return JSONResponse(
            status_code=422,
            content=ErrorResponse(error_code=error_code, detail=message).model_dump(),
        )

    return EmbeddingResponse(
        success=True,
        embedding=embedding.tolist(),
        dimension=len(embedding),
        model=ARCFACE_RESNET.filename,
        model_sha256=ARCFACE_RESNET.sha256,
    )


@app.post(
    "/v1/face/verify",
    response_model=VerifyResponse,
    responses={
        422: {"model": ErrorResponse},
        503: {"model": ErrorResponse},
    },
)
async def verify_face(
    image: UploadFile = File(...),
    reference_embedding: str = Form(..., alias="referenceEmbedding"),
    threshold: float | None = Form(default=None),
):
    if not engine.is_ready:
        return JSONResponse(
            status_code=503,
            content=ErrorResponse(
                error_code="SERVICE_NOT_READY",
                detail="Face models are not loaded. Check /health/ready.",
            ).model_dump(),
        )

    # --- Validate threshold, if supplied. .NET is the business owner of
    # this value in production; Python only enforces that whatever it's
    # handed is a sane number, never silently substitutes or clamps it. ---
    if threshold is not None and not (0.0 <= threshold <= 1.0):
        return JSONResponse(
            status_code=422,
            content=ErrorResponse(
                error_code="INVALID_THRESHOLD",
                detail=f"threshold must be between 0.0 and 1.0, got {threshold}.",
            ).model_dump(),
        )

    effective_threshold = threshold if threshold is not None else DEFAULT_VERIFICATION_THRESHOLD

    # --- Validate the reference embedding: must parse as JSON, must be a
    # list of exactly EMBEDDING_DIM numeric values. ---
    try:
        parsed = json.loads(reference_embedding)
    except (json.JSONDecodeError, TypeError):
        return JSONResponse(
            status_code=422,
            content=ErrorResponse(
                error_code="INVALID_REFERENCE_EMBEDDING",
                detail="referenceEmbedding must be a JSON array of floats.",
            ).model_dump(),
        )

    if not isinstance(parsed, list) or len(parsed) != EMBEDDING_DIM:
        return JSONResponse(
            status_code=422,
            content=ErrorResponse(
                error_code="INVALID_REFERENCE_EMBEDDING",
                detail=f"referenceEmbedding must contain exactly {EMBEDDING_DIM} numeric values, got {len(parsed) if isinstance(parsed, list) else 'non-list input'}.",
            ).model_dump(),
        )

    try:
        reference_vector = np.array(parsed, dtype=np.float32)
    except (ValueError, TypeError):
        return JSONResponse(
            status_code=422,
            content=ErrorResponse(
                error_code="INVALID_REFERENCE_EMBEDDING",
                detail="referenceEmbedding contains non-numeric values.",
            ).model_dump(),
        )

    # --- Detect + embed the incoming image, same controlled failure modes
    # as /v1/face/embedding. ---
    image_bytes = await image.read()

    try:
        candidate_embedding = engine.get_single_face_embedding(image_bytes)
    except FaceDetectionError as exc:
        message = str(exc)
        if "Could not decode" in message:
            error_code = "INVALID_IMAGE"
        elif "Multiple faces" in message:
            error_code = "MULTIPLE_FACES_DETECTED"
        else:
            error_code = "NO_FACE_DETECTED"

        return JSONResponse(
            status_code=422,
            content=ErrorResponse(error_code=error_code, detail=message).model_dump(),
        )

    # --- Compare. Full floating-point precision, no rounding before the
    # comparison — only the serialized response value is naturally limited
    # by JSON's float representation, never explicitly truncated by us. ---
    similarity = engine.similarity(candidate_embedding, reference_vector)
    matched = similarity >= effective_threshold

    return VerifyResponse(
        success=True,
        matched=matched,
        similarity=similarity,
        threshold=effective_threshold,
        model=ARCFACE_RESNET.filename,
        model_sha256=ARCFACE_RESNET.sha256,
        face_count=1,
    )