using ACC.Application.Application.UseCases.CompleteOnboarding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ACC.Application.Infrastructure.Endpoints;

internal static class OnboardingEndpoints
{
    public static IEndpointRouteBuilder MapOnboardingEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/complete", (
            CompleteOnboardingCommand command,
            CompleteOnboardingHandler handler) =>
        {
            try
            {
                var result = handler.Handle(command);

                return Results.Created(
                    $"/accounting-subjects/{result.AccountingSubjectId}",
                    result);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                return Results.Problem(
                    detail: exception.Message,
                    statusCode: StatusCodes.Status400BadRequest);
            }
        })
        .WithName("CompleteOnboarding")
        .WithTags("Onboarding")
        .WithSummary("Complete onboarding")
        .WithDescription(
            "Creates an accounting subject, establishes its founding Owner, and adopts the selected chart-of-accounts template.")
        .Produces<CompleteOnboardingResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return endpoints;
    }
}
