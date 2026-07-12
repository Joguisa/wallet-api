using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WalletApi.Domain.Shared;

namespace WalletApi.Api.Middleware;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            ValidationException validationException => FromValidation(validationException),
            DomainException domainException => FromDomain(domainException),
            DbUpdateConcurrencyException => Create(
                StatusCodes.Status409Conflict,
                "CONCURRENCY_CONFLICT",
                "Concurrency conflict",
                "The wallet was modified by another operation. Please retry."),
            DbUpdateException dbUpdateException when IsUniqueConstraintViolation(dbUpdateException) =>
                FromUniqueConstraintViolation(dbUpdateException),
            _ => Create(
                StatusCodes.Status500InternalServerError,
                "INTERNAL_ERROR",
                "Internal server error",
                "An unexpected error occurred.")
        };

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            _logger.LogWarning("Request failed with {ErrorCode}: {Message}",
                problemDetails.Extensions["errorCode"], exception.Message);
        }

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails FromValidation(ValidationException exception)
    {
        var problemDetails = Create(
            StatusCodes.Status400BadRequest,
            "VALIDATION_ERROR",
            "Validation failed",
            "One or more validation errors occurred.");

        problemDetails.Extensions["errors"] = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return problemDetails;
    }

    private static ProblemDetails FromDomain(DomainException exception)
    {
        var status = exception.ErrorCode switch
        {
            "WALLET_NOT_FOUND" => StatusCodes.Status404NotFound,
            "DUPLICATE_DOCUMENT_ID" => StatusCodes.Status409Conflict,
            "INSUFFICIENT_FUNDS" => StatusCodes.Status422UnprocessableEntity,
            "IDEMPOTENCY_KEY_REUSE" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };

        var title = status switch
        {
            StatusCodes.Status404NotFound => "Resource not found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status422UnprocessableEntity => "Business rule violated",
            _ => "Invalid request"
        };

        return Create(status, exception.ErrorCode, title, exception.Message);
    }

    private static ProblemDetails FromUniqueConstraintViolation(DbUpdateException exception)
    {
        var isDocumentId = exception.InnerException?.Message.Contains("UX_Wallets_DocumentId") == true;

        return Create(
            StatusCodes.Status409Conflict,
            isDocumentId ? "DUPLICATE_DOCUMENT_ID" : "CONFLICT",
            "Conflict",
            isDocumentId
                ? "A wallet for this document id already exists."
                : "The request conflicts with existing data.");
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException?.Message is { } message &&
        (message.Contains("duplicate key") || message.Contains("UNIQUE KEY") || message.Contains("unique index"));

    private static ProblemDetails Create(int status, string errorCode, string title, string detail)
    {
        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };

        problemDetails.Extensions["errorCode"] = errorCode;

        return problemDetails;
    }
}
