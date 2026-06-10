using ACC.Ledger.Application.UseCases.CloseFiscalPeriod;
using ACC.Ledger.Application.UseCases.OpenFiscalPeriod;
using ACC.Ledger.Application.UseCases.PostJournalEntry;
using ACC.Ledger.Application.UseCases.ViewJournalEntry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ACC.Ledger.Infrastructure.Endpoints;

internal static class LedgerEndpoints
{
    public static IEndpointRouteBuilder MapLedgerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/fiscal-periods", (
            OpenFiscalPeriodCommand command,
            OpenFiscalPeriodHandler handler) =>
        {
            try
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Created($"/ledger/fiscal-periods/{result.FiscalPeriodId}", result);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                return BadRequest(exception);
            }
        })
        .WithName("OpenFiscalPeriod")
        .WithTags("Ledger")
        .WithSummary("Open a fiscal period")
        .WithDescription("Opens a fiscal period for an accounting subject.")
        .Produces<OpenFiscalPeriodResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/fiscal-periods/{fiscalPeriodId:guid}/close", (
            Guid fiscalPeriodId,
            CloseFiscalPeriodHandler handler) =>
        {
            try
            {
                var result = handler.Handle(
                    new CloseFiscalPeriodCommand(fiscalPeriodId),
                    DateTimeOffset.UtcNow);

                return Results.Ok(result);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                return BadRequest(exception);
            }
        })
        .WithName("CloseFiscalPeriod")
        .WithTags("Ledger")
        .WithSummary("Close a fiscal period")
        .WithDescription("Closes an open fiscal period.")
        .Produces<CloseFiscalPeriodResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/journal-entries", (
            PostJournalEntryCommand command,
            PostJournalEntryHandler handler) =>
        {
            try
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Created($"/ledger/journal-entries/{result.JournalEntryId}", result);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                return BadRequest(exception);
            }
        })
        .WithName("PostJournalEntry")
        .WithTags("Ledger")
        .WithSummary("Post a journal entry")
        .WithDescription("Records a balanced journal entry in an open posting period.")
        .Produces<PostJournalEntryResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapGet("/journal-entries/{journalEntryId:guid}", (
            Guid journalEntryId,
            ViewJournalEntryHandler handler) =>
        {
            var journalEntry = handler.Handle(new ViewJournalEntryQuery(journalEntryId));

            return journalEntry is null
                ? Results.NotFound()
                : Results.Ok(journalEntry);
        })
        .WithName("ViewJournalEntry")
        .WithTags("Ledger")
        .WithSummary("View a journal entry")
        .WithDescription("Returns the projected view of a posted journal entry.")
        .Produces<ViewJournalEntryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static IResult BadRequest(Exception exception) =>
        Results.Problem(
            detail: exception.Message,
            statusCode: StatusCodes.Status400BadRequest);
}
