"""
Response schemas for the FieldWork Face Service.

Kept deliberately minimal and opinion-free: this service reports facts
(embedding vectors, similarity scores, face counts) and does not make
pass/fail attendance decisions. That decision belongs to FieldWork.Api's
AttendanceService, per the Phase 5 design principle: Python provides ML
results, .NET applies business rules.
"""

from pydantic import BaseModel


class EmbeddingResponse(BaseModel):
    success: bool
    embedding: list[float]
    dimension: int
    model: str
    model_sha256: str


class VerifyResponse(BaseModel):
    success: bool
    matched: bool
    similarity: float
    threshold: float
    model: str
    model_sha256: str
    face_count: int


class ErrorResponse(BaseModel):
    success: bool = False
    error_code: str
    detail: str
