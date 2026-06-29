using ACC.Identity.Application.UseCases.AuthenticateUser;
using ACC.Identity.Application.UseCases.RegisterUser;
using ACC.Identity.Application.UseCases.ResendVerification;
using ACC.Identity.Application.UseCases.VerifyEmail;
using ACC.Identity.Domain.Invariants;
using ACC.BuildingBlocks.Domain;
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
            return Execute(() =>
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Created($"/identity/users/{result.UserId}", result);
            });
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
            return Execute(() =>
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Ok(result);
            });
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
            return Execute(() =>
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Ok(result);
            });
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
            return Execute(() =>
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Ok(result);
            });
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

    private static IResult Execute(Func<IResult> act)
    {
        try
        {
            return act();
        }
        catch (InvariantViolationException exception)
        {
            return InvariantProblem(exception);
        }
    }

    private static IResult InvariantProblem(InvariantViolationException exception) =>
        Problem(exception, exception switch
        {
            UserEmailMustBeValidViolation => StatusCodes.Status422UnprocessableEntity,
            UserEmailMustBeUniqueViolation => StatusCodes.Status409Conflict,
            EmailVerificationMustBeValidViolation => StatusCodes.Status422UnprocessableEntity,
            EmailMustNotAlreadyBeVerifiedViolation => StatusCodes.Status409Conflict,
            UserMustBeActiveToAuthenticateViolation => StatusCodes.Status401Unauthorized,
            _ => throw new InvalidOperationException(
                $"Invariant violation {exception.GetType().Name} has no Identity HTTP mapping.",
                exception)
        });

    private static IResult Problem(Exception exception, int statusCode) =>
        Results.Problem(
            detail: exception.Message,
            statusCode: statusCode);
}
