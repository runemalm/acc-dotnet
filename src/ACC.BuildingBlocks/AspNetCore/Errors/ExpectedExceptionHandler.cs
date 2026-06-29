using ACC.BuildingBlocks.Authorization;
using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ACC.BuildingBlocks.AspNetCore.Errors;

public sealed class ExpectedExceptionHandler(IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            AuthenticationFailedException => StatusCodes.Status401Unauthorized,
            AuthorizationDeniedException => StatusCodes.Status403Forbidden,
            RequiredObjectNotFoundException => StatusCodes.Status404NotFound,
            WrongExpectedStreamVersionException => StatusCodes.Status409Conflict,
            ApplicationValidationException => StatusCodes.Status422UnprocessableEntity,
            _ => (int?)null
        };

        if (statusCode is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode.Value;

        var detail = exception is WrongExpectedStreamVersionException
            ? "The operation conflicts with changes made since its state was read."
            : exception.Message;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Detail = detail,
                Instance = httpContext.Request.Path
            },
            Exception = exception
        });
    }
}
