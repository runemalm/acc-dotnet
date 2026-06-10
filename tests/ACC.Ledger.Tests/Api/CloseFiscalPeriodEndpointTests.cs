using System.Net;
using System.Net.Http.Json;
using ACC.Ledger.Application.UseCases.CloseFiscalPeriod;
using ACC.Ledger.Application.UseCases.OpenFiscalPeriod;
using ACC.Ledger.Tests.TestKit;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.Ledger.Tests.Api;

public sealed class CloseFiscalPeriodEndpointTests
{
    [Fact]
    public async Task CloseFiscalPeriod_WithOpenPeriod_ReturnsOk()
    {
        await using var context = await LedgerApiTestContext.Create();
        var opened = await OpenFiscalPeriod(context);

        var response = await context.Client.PostAsync(
            $"/ledger/fiscal-periods/{opened.FiscalPeriodId}/close",
            content: null);

        var result = await response.Content.ReadFromJsonAsync<CloseFiscalPeriodResult>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(opened.FiscalPeriodId, result.FiscalPeriodId);
    }

    [Fact]
    public async Task CloseFiscalPeriod_WithUnknownPeriod_ReturnsBadRequest()
    {
        await using var context = await LedgerApiTestContext.Create();

        var response = await context.Client.PostAsync(
            $"/ledger/fiscal-periods/{Guid.NewGuid()}/close",
            content: null);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem.Status);
        Assert.Contains("could not be found", problem.Detail);
    }

    private static async Task<OpenFiscalPeriodResult> OpenFiscalPeriod(LedgerApiTestContext context)
    {
        var response = await context.Client.PostAsJsonAsync(
            "/ledger/fiscal-periods",
            new OpenFiscalPeriodCommand(
                Guid.NewGuid(),
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31)));

        return (await response.Content.ReadFromJsonAsync<OpenFiscalPeriodResult>())!;
    }
}
