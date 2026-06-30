using ACC.Ledger.Application.UseCases.CloseFiscalPeriod;
using ACC.Ledger.Application.UseCases.OpenFiscalPeriod;
using ACC.Ledger.Application.UseCases.PostJournalEntry;
using ACC.Ledger.Application.UseCases.ViewJournalEntry;
using ACC.Ledger.Domain.Invariants;
using ACC.BuildingBlocks.Domain;
using ACC.BuildingBlocks.Failures;
using ACC.BuildingBlocks.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ACC.Ledger.Infrastructure.Endpoints;

internal static class LedgerEndpoints
{
    public static IEndpointRouteBuilder MapLedgerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/fiscal-periods", (
            OpenFiscalPeriodRequest request,
            HttpContext context,
            OpenFiscalPeriodHandler handler) =>
        {
            return Execute(() =>
            {
                var command = new OpenFiscalPeriodCommand(
                    context.User.GetRequiredUserId(),
                    request.AccountingSubjectId,
                    request.StartsOn,
                    request.EndsOn);
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Created($"/ledger/fiscal-periods/{result.FiscalPeriodId}", result);
            });
        })
        .WithName("OpenFiscalPeriod")
        .WithTags("Ledger")
        .WithSummary("Open a fiscal period")
        .WithDescription("Opens a fiscal period for an accounting subject.")
        .Produces<OpenFiscalPeriodResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        endpoints.MapPost("/fiscal-periods/{fiscalPeriodId:guid}/close", (
            Guid fiscalPeriodId,
            HttpContext context,
            CloseFiscalPeriodHandler handler) =>
        {
            return Execute(() =>
            {
                var result = handler.Handle(
                    new CloseFiscalPeriodCommand(
                        context.User.GetRequiredUserId(),
                        fiscalPeriodId),
                    DateTimeOffset.UtcNow);

                return Results.Ok(result);
            });
        })
        .WithName("CloseFiscalPeriod")
        .WithTags("Ledger")
        .WithSummary("Close a fiscal period")
        .WithDescription("Closes an open fiscal period.")
        .Produces<CloseFiscalPeriodResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        endpoints.MapPost("/journal-entries", (
            PostJournalEntryRequest request,
            HttpContext context,
            PostJournalEntryHandler handler) =>
        {
            return Execute(() =>
            {
                if (request.Lines is null || request.Lines.Any(line => line is null))
                {
                    throw new ApplicationValidationException(
                        "A journal entry must have at least one line.");
                }

                var command = new PostJournalEntryCommand(
                    context.User.GetRequiredUserId(),
                    request.AccountingSubjectId,
                    request.AccountingDate,
                    request.Description,
                    request.Lines
                        .Select(line => new PostJournalEntryCommandLine(
                            line.Account,
                            line.Debit,
                            line.Credit))
                        .ToArray());
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Created($"/ledger/journal-entries/{result.JournalEntryId}", result);
            });
        })
        .WithName("PostJournalEntry")
        .WithTags("Ledger")
        .WithSummary("Post a journal entry")
        .WithDescription("Records a balanced journal entry in an open posting period.")
        .Produces<PostJournalEntryResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        endpoints.MapGet("/journal-entries/{journalEntryId:guid}", (
            Guid journalEntryId,
            HttpContext context,
            ViewJournalEntryHandler handler) =>
        {
            return Execute(() =>
            {
                var journalEntry = handler.Handle(new ViewJournalEntryQuery(
                    context.User.GetRequiredUserId(),
                    journalEntryId));

                return journalEntry is null
                    ? Results.NotFound()
                    : Results.Ok(journalEntry);
            });
        })
        .WithName("ViewJournalEntry")
        .WithTags("Ledger")
        .WithSummary("View a journal entry")
        .WithDescription("Returns the projected view of a posted journal entry.")
        .Produces<ViewJournalEntryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
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
            JournalEntryMustBalanceViolation => StatusCodes.Status422UnprocessableEntity,
            PostingAccountMustBeRecognizedViolation => StatusCodes.Status422UnprocessableEntity,
            PostingMustOccurInOpenPeriodViolation => StatusCodes.Status409Conflict,
            PostingAccountMustBeActiveViolation => StatusCodes.Status409Conflict,
            FiscalPeriodMustBeOpenToCloseViolation => StatusCodes.Status409Conflict,
            _ => throw new InvalidOperationException(
                $"Invariant violation {exception.GetType().Name} has no Ledger HTTP mapping.",
                exception)
        });

    private static IResult Problem(Exception exception, int statusCode) =>
        Results.Problem(
            detail: exception.Message,
            statusCode: statusCode);

}

public sealed record OpenFiscalPeriodRequest(
    Guid AccountingSubjectId,
    DateOnly StartsOn,
    DateOnly EndsOn);

public sealed record PostJournalEntryRequest(
    Guid AccountingSubjectId,
    DateOnly AccountingDate,
    string Description,
    IReadOnlyCollection<PostJournalEntryRequestLine> Lines);

public sealed record PostJournalEntryRequestLine(
    string Account,
    decimal Debit,
    decimal Credit);
