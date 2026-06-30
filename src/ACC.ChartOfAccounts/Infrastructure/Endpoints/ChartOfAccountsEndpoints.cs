using ACC.ChartOfAccounts.Application.UseCases.AddAccount;
using ACC.ChartOfAccounts.Application.UseCases.AdoptChartOfAccounts;
using ACC.ChartOfAccounts.Application.UseCases.DeactivateAccount;
using ACC.ChartOfAccounts.Application.UseCases.ReactivateAccount;
using ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccounts;
using ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccountsTemplates;
using ACC.ChartOfAccounts.Domain.Invariants;
using ACC.BuildingBlocks.Domain;
using ACC.BuildingBlocks.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ACC.ChartOfAccounts.Infrastructure.Endpoints;

internal static class ChartOfAccountsEndpoints
{
    public static IEndpointRouteBuilder MapChartOfAccountsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/adopt", (
            AdoptChartOfAccountsRequest request,
            HttpContext context,
            AdoptChartOfAccountsHandler handler) => Execute(() =>
        {
            var command = new AdoptChartOfAccountsCommand(
                context.User.GetRequiredUserId(),
                request.AccountingSubjectId,
                request.TemplateId);
            var result = handler.Handle(command, DateTimeOffset.UtcNow);

            return Results.Created(
                $"/chart-of-accounts/accounting-subjects/{command.AccountingSubjectId}",
                result);
        }))
        .WithName("AdoptChartOfAccounts")
        .WithTags("Chart of Accounts")
        .WithSummary("Adopt a chart of accounts")
        .WithDescription("Adopts an identified chart-of-accounts template for an accounting subject.")
        .Produces<AdoptChartOfAccountsResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        endpoints.MapPost("/add-account", (
            AddAccountRequest request,
            HttpContext context,
            AddAccountHandler handler) => Execute(
                () => Results.Ok(handler.Handle(
                    new AddAccountCommand(
                        context.User.GetRequiredUserId(),
                        request.ChartOfAccountsId,
                        request.AccountNumber,
                        request.AccountName),
                    DateTimeOffset.UtcNow))))
        .WithName("AddAccount")
        .WithTags("Chart of Accounts")
        .WithSummary("Add an account")
        .WithDescription("Adds a locally defined account to an adopted chart of accounts.")
        .Produces<AddAccountResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        endpoints.MapPost("/deactivate-account", (
            ChangeAccountAvailabilityRequest request,
            HttpContext context,
            DeactivateAccountHandler handler) => Execute(
                () => Results.Ok(handler.Handle(
                    new DeactivateAccountCommand(
                        context.User.GetRequiredUserId(),
                        request.ChartOfAccountsId,
                        request.AccountNumber),
                    DateTimeOffset.UtcNow))))
        .WithName("DeactivateAccount")
        .WithTags("Chart of Accounts")
        .WithSummary("Deactivate an account")
        .WithDescription("Ends an account's availability for future posting.")
        .Produces<DeactivateAccountResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        endpoints.MapPost("/reactivate-account", (
            ChangeAccountAvailabilityRequest request,
            HttpContext context,
            ReactivateAccountHandler handler) => Execute(
                () => Results.Ok(handler.Handle(
                    new ReactivateAccountCommand(
                        context.User.GetRequiredUserId(),
                        request.ChartOfAccountsId,
                        request.AccountNumber),
                    DateTimeOffset.UtcNow))))
        .WithName("ReactivateAccount")
        .WithTags("Chart of Accounts")
        .WithSummary("Reactivate an account")
        .WithDescription("Restores an account's availability for future posting.")
        .Produces<ReactivateAccountResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        endpoints.MapGet("/templates", (
            ViewChartOfAccountsTemplatesHandler handler) =>
            Results.Ok(handler.Handle(new ViewChartOfAccountsTemplatesQuery())))
        .WithName("ViewChartOfAccountsTemplates")
        .WithTags("Chart of Accounts")
        .WithSummary("View chart of accounts templates")
        .WithDescription("Returns the chart-of-accounts templates currently available for adoption.")
        .Produces<ViewChartOfAccountsTemplatesResponse>(StatusCodes.Status200OK);

        endpoints.MapGet("/accounting-subjects/{accountingSubjectId:guid}", (
            Guid accountingSubjectId,
            HttpContext context,
            ViewChartOfAccountsHandler handler) => Execute(() =>
        {
            var result = handler.Handle(new ViewChartOfAccountsQuery(
                context.User.GetRequiredUserId(),
                accountingSubjectId));

            return result is null ? Results.NotFound() : Results.Ok(result);
        }))
        .WithName("ViewChartOfAccounts")
        .WithTags("Chart of Accounts")
        .WithSummary("View a chart of accounts")
        .WithDescription("Returns the adopted chart of accounts for an accounting subject.")
        .Produces<ViewChartOfAccountsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
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
            AccountMustBeRecognizedByChartOfAccountsViolation =>
                StatusCodes.Status404NotFound,
            AccountingSubjectMustHaveAtMostOneOperativeChartOfAccountsViolation =>
                StatusCodes.Status409Conflict,
            AccountNumberMustBeUniqueWithinChartOfAccountsViolation =>
                StatusCodes.Status409Conflict,
            AccountMustBeActiveToDeactivateViolation => StatusCodes.Status409Conflict,
            AccountMustBeInactiveToReactivateViolation => StatusCodes.Status409Conflict,
            _ => throw new InvalidOperationException(
                $"Invariant violation {exception.GetType().Name} has no Chart of Accounts HTTP mapping.",
                exception)
        });

    private static IResult Problem(Exception exception, int statusCode) =>
        Results.Problem(
            detail: exception.Message,
            statusCode: statusCode);

}

public sealed record AdoptChartOfAccountsRequest(
    Guid AccountingSubjectId,
    string TemplateId);

public sealed record AddAccountRequest(
    Guid ChartOfAccountsId,
    string AccountNumber,
    string AccountName);

public sealed record ChangeAccountAvailabilityRequest(
    Guid ChartOfAccountsId,
    string AccountNumber);
