using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Middleware;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception is DomainException domainException
            ? domainException.StatusCode
            : StatusCodes.Status500InternalServerError;

        if (statusCode >= 500)
        {
            logger.LogError(exception, "Unhandled API exception for {Path}", httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation("Request rejected with {StatusCode}: {Message}", statusCode, exception.Message);
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode >= 500 ? "An unexpected error occurred." : "Request could not be completed.",
            Detail = exception is DomainException || environment.IsDevelopment()
                ? exception.Message
                : null,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
