using ACC.Identity.Application.UseCases.AuthenticateUser;
using ACC.Identity.Application.UseCases.RegisterUser;
using ACC.Identity.Application.UseCases.ResendVerification;
using ACC.Identity.Application.UseCases.VerifyEmail;
using ACC.BuildingBlocks.Failures;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ACC.Identity.Infrastructure.Endpoints;

internal static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/register", (
            RegisterUserCommand command,
            RegisterUserHandler handler) =>
        {
            try
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Created($"/identity/users/{result.UserId}", result);
            }
            catch (StateConflictException exception)
            {
                return Problem(exception, StatusCodes.Status409Conflict);
            }
            catch (SemanticViolationException exception)
            {
                return Problem(exception, StatusCodes.Status422UnprocessableEntity);
            }
            catch (ArgumentException exception)
            {
                return Problem(exception, StatusCodes.Status422UnprocessableEntity);
            }
        })
        .WithName("RegisterUser")
        .WithTags("Identity")
        .WithSummary("Register a user")
        .WithDescription("Registers a user identity, starts email verification, and issues an authentication token.")
        .Produces<RegisterUserResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        endpoints.MapPost("/authenticate", (
            AuthenticateUserCommand command,
            AuthenticateUserHandler handler) =>
        {
            try
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Ok(result);
            }
            catch (AuthenticationFailedException exception)
            {
                return Problem(exception, StatusCodes.Status401Unauthorized);
            }
            catch (ArgumentException exception)
            {
                return Problem(exception, StatusCodes.Status422UnprocessableEntity);
            }
        })
        .WithName("AuthenticateUser")
        .WithTags("Identity")
        .WithSummary("Authenticate a user")
        .WithDescription("Authenticates a user using their email and password credential.")
        .Produces<AuthenticateUserResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        endpoints.MapPost("/verify-email", (
            VerifyEmailCommand command,
            VerifyEmailHandler handler) =>
        {
            try
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Ok(result);
            }
            catch (SemanticViolationException exception)
            {
                return Problem(exception, StatusCodes.Status422UnprocessableEntity);
            }
            catch (ArgumentException exception)
            {
                return Problem(exception, StatusCodes.Status422UnprocessableEntity);
            }
        })
        .WithName("VerifyEmail")
        .WithTags("Identity")
        .WithSummary("Verify email")
        .WithDescription("Confirms that a user controls the registered email address.")
        .Produces<VerifyEmailResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        endpoints.MapPost("/resend-verification", (
            ResendVerificationCommand command,
            ResendVerificationHandler handler) =>
        {
            try
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Ok(result);
            }
            catch (SemanticViolationException exception)
            {
                return Problem(exception, StatusCodes.Status422UnprocessableEntity);
            }
            catch (ArgumentException exception)
            {
                return Problem(exception, StatusCodes.Status422UnprocessableEntity);
            }
        })
        .WithName("ResendVerification")
        .WithTags("Identity")
        .WithSummary("Resend verification")
        .WithDescription("Issues a new email verification token for an unverified user.")
        .Produces<ResendVerificationResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static IResult Problem(Exception exception, int statusCode) =>
        Results.Problem(
            detail: exception.Message,
            statusCode: statusCode);
}
