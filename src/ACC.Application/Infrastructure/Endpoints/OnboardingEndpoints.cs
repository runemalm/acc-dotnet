using ACC.Application.Application.UseCases.CompleteOnboarding;
using ACC.AccountingSubject.Domain.Aggregates;
using ACC.BuildingBlocks.Authorization;
using ACC.BuildingBlocks.Failures;
using ACC.BuildingBlocks.Security;
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
            CompleteOnboardingRequest request,
            HttpContext context,
            CompleteOnboardingHandler handler) =>
        {
            try
            {
                var command = new CompleteOnboardingCommand(
                    context.User.GetRequiredUserId(),
                    request.AccountingSubjectName,
                    request.OrganizationNumber,
                    request.AccountingSubjectType,
                    request.Country,
                    request.AccountingMethod,
                    request.VatReportingPeriod,
                    request.ChartOfAccountsTemplateId);
                var result = handler.Handle(command);

                return Results.Created(
                    $"/accounting-subjects/{result.AccountingSubjectId}",
                    result);
            }
            catch (AuthorizationDeniedException exception)
            {
                return Results.Problem(
                    detail: exception.Message,
                    statusCode: StatusCodes.Status403Forbidden);
            }
            catch (ResourceNotFoundException exception)
            {
                return Results.Problem(
                    detail: exception.Message,
                    statusCode: StatusCodes.Status404NotFound);
            }
            catch (StateConflictException exception)
            {
                return Results.Problem(
                    detail: exception.Message,
                    statusCode: StatusCodes.Status409Conflict);
            }
            catch (Exception exception) when (exception is SemanticViolationException or ArgumentException)
            {
                return Results.Problem(
                    detail: exception.Message,
                    statusCode: StatusCodes.Status422UnprocessableEntity);
            }
        })
        .WithName("CompleteOnboarding")
        .WithTags("Onboarding")
        .WithSummary("Complete onboarding")
        .WithDescription(
            "Creates an accounting subject, establishes its founding Owner, and adopts the selected chart-of-accounts template.")
        .Produces<CompleteOnboardingResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }
}

public sealed record CompleteOnboardingRequest(
    string AccountingSubjectName,
    string OrganizationNumber,
    AccountingSubjectType AccountingSubjectType,
    Country Country,
    AccountingMethod AccountingMethod,
    VatReportingPeriod VatReportingPeriod,
    string ChartOfAccountsTemplateId);
