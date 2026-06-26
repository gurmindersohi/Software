"""Domain exceptions — raised by the core/service layer, translated to HTTP at
the edge (see app.main exception handler). Keeps business logic free of FastAPI."""


class AppError(Exception):
    status_code = 400

    def __init__(self, detail: str) -> None:
        self.detail = detail
        super().__init__(detail)


class BadRequestError(AppError):
    status_code = 400


class PermissionDeniedError(AppError):
    status_code = 403


class NotFoundError(AppError):
    status_code = 404


class ConflictError(AppError):
    status_code = 409


class PaymentRequiredError(AppError):
    status_code = 402  # trial expired / on hold / quota exceeded
