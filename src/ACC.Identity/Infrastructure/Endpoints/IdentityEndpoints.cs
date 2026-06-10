using ACC.Identity.Application.UseCases.AuthenticateUser;
using ACC.Identity.Application.UseCases.RegisterUser;
using ACC.Identity.Application.UseCases.ResendVerification;
using ACC.Identity.Application.UseCases.VerifyEmail;
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
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                return BadRequest(exception);
            }
        })
        .WithName("RegisterUser")
        .WithTags("Identity")
        .WithSummary("Register a user")
        .WithDescription("Registers a user identity, starts email verification, and issues an authentication token.")
        .Produces<RegisterUserResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/authenticate", (
            AuthenticateUserCommand command,
            AuthenticateUserHandler handler) =>
        {
            try
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Ok(result);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                return BadRequest(exception);
            }
        })
        .WithName("AuthenticateUser")
        .WithTags("Identity")
        .WithSummary("Authenticate a user")
        .WithDescription("Authenticates a user using their email and password credential.")
        .Produces<AuthenticateUserResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/verify-email", (
            VerifyEmailCommand command,
            VerifyEmailHandler handler) =>
        {
            try
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Ok(result);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                return BadRequest(exception);
            }
        })
        .WithName("VerifyEmail")
        .WithTags("Identity")
        .WithSummary("Verify email")
        .WithDescription("Confirms that a user controls the registered email address.")
        .Produces<VerifyEmailResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/resend-verification", (
            ResendVerificationCommand command,
            ResendVerificationHandler handler) =>
        {
            try
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Ok(result);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                return BadRequest(exception);
            }
        })
        .WithName("ResendVerification")
        .WithTags("Identity")
        .WithSummary("Resend verification")
        .WithDescription("Issues a new email verification token for an unverified user.")
        .Produces<ResendVerificationResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static IResult BadRequest(Exception exception) =>
        Results.Problem(
            detail: exception.Message,
            statusCode: StatusCodes.Status400BadRequest);
}
