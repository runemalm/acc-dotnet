using ACC.ChartOfAccounts.Application.UseCases.AddAccount;
using ACC.ChartOfAccounts.Application.UseCases.AdoptChartOfAccounts;
using ACC.ChartOfAccounts.Application.UseCases.DeactivateAccount;
using ACC.ChartOfAccounts.Application.UseCases.ReactivateAccount;
using ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccounts;
using ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccountsTemplates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ACC.ChartOfAccounts.Infrastructure.Endpoints;

internal static class ChartOfAccountsEndpoints
{
    public static IEndpointRouteBuilder MapChartOfAccountsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/adopt", (
            AdoptChartOfAccountsCommand command,
            AdoptChartOfAccountsHandler handler) =>
        {
            try
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);
                return Results.Created(
                    $"/chart-of-accounts/accounting-subjects/{command.AccountingSubjectId}",
                    result);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                return BadRequest(exception);
            }
        })
        .WithName("AdoptChartOfAccounts")
        .WithTags("Chart of Accounts")
        .WithSummary("Adopt a chart of accounts")
        .WithDescription("Adopts an identified chart-of-accounts template for an accounting subject.")
        .Produces<AdoptChartOfAccountsResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/add-account", (
            AddAccountCommand command,
            AddAccountHandler handler) => Execute(
                () => Results.Ok(handler.Handle(command, DateTimeOffset.UtcNow))))
        .WithName("AddAccount")
        .WithTags("Chart of Accounts")
        .WithSummary("Add an account")
        .WithDescription("Adds a locally defined account to an adopted chart of accounts.")
        .Produces<AddAccountResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/deactivate-account", (
            DeactivateAccountCommand command,
            DeactivateAccountHandler handler) => Execute(
                () => Results.Ok(handler.Handle(command, DateTimeOffset.UtcNow))))
        .WithName("DeactivateAccount")
        .WithTags("Chart of Accounts")
        .WithSummary("Deactivate an account")
        .WithDescription("Ends an account's availability for future posting.")
        .Produces<DeactivateAccountResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/reactivate-account", (
            ReactivateAccountCommand command,
            ReactivateAccountHandler handler) => Execute(
                () => Results.Ok(handler.Handle(command, DateTimeOffset.UtcNow))))
        .WithName("ReactivateAccount")
        .WithTags("Chart of Accounts")
        .WithSummary("Reactivate an account")
        .WithDescription("Restores an account's availability for future posting.")
        .Produces<ReactivateAccountResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

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
            ViewChartOfAccountsHandler handler) =>
        {
            var result = handler.Handle(new ViewChartOfAccountsQuery(accountingSubjectId));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("ViewChartOfAccounts")
        .WithTags("Chart of Accounts")
        .WithSummary("View a chart of accounts")
        .WithDescription("Returns the adopted chart of accounts for an accounting subject.")
        .Produces<ViewChartOfAccountsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static IResult Execute(Func<IResult> act)
    {
        try
        {
            return act();
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return BadRequest(exception);
        }
    }

    private static IResult BadRequest(Exception exception) =>
        Results.Problem(
            detail: exception.Message,
            statusCode: StatusCodes.Status400BadRequest);
}
