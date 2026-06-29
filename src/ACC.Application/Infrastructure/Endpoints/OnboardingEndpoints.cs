using ACC.Application.Application.UseCases.CompleteOnboarding;
using ACC.AccountingSubject.Domain.Aggregates;
using ACC.Authority.Domain.Invariants;
using ACC.BuildingBlocks.Domain;
using ACC.ChartOfAccounts.Domain.Invariants;
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
            return Execute(() =>
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
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Created(
                    $"/accounting-subjects/{result.AccountingSubjectId}",
                    result);
            });
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
            UserMustBeRecognizedForAuthorityViolation => StatusCodes.Status404NotFound,
            AccountingSubjectMustBeRecognizedForAuthorityViolation => StatusCodes.Status404NotFound,
            ActiveRoleAssignmentMustBeUniqueViolation => StatusCodes.Status409Conflict,
            AccountingSubjectMustBeRecognizedForChartOfAccountsViolation =>
                StatusCodes.Status404NotFound,
            ActorMustHaveChartOfAccountsPowerViolation => StatusCodes.Status403Forbidden,
            AccountingSubjectMustHaveAtMostOneOperativeChartOfAccountsViolation =>
                StatusCodes.Status409Conflict,
            AccountNumberMustBeUniqueWithinChartOfAccountsViolation =>
                StatusCodes.Status409Conflict,
            _ => throw new InvalidOperationException(
                $"Invariant violation {exception.GetType().Name} has no onboarding HTTP mapping.",
                exception)
        });

    private static IResult Problem(Exception exception, int statusCode) =>
        Results.Problem(
            detail: exception.Message,
            statusCode: statusCode);
}

public sealed record CompleteOnboardingRequest(
    string AccountingSubjectName,
    string OrganizationNumber,
    AccountingSubjectType AccountingSubjectType,
    Country Country,
    AccountingMethod AccountingMethod,
    VatReportingPeriod VatReportingPeriod,
    string ChartOfAccountsTemplateId);
