using System.Net;
using System.Net.Http.Json;
using ACC.Ledger.Application.UseCases.OpenFiscalPeriod;
using ACC.Ledger.Tests.TestKit;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.Ledger.Tests.Api;

public sealed class OpenFiscalPeriodEndpointTests
{
    [Fact]
    public async Task OpenFiscalPeriod_WithValidPeriod_ReturnsCreated()
    {
        await using var context = await LedgerApiTestContext.Create();

        var response = await context.Client.PostAsJsonAsync(
            "/ledger/fiscal-periods",
            new OpenFiscalPeriodCommand(
                Guid.NewGuid(),
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31)));

        var result = await response.Content.ReadFromJsonAsync<OpenFiscalPeriodResult>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.FiscalPeriodId);
        Assert.Equal(
            new Uri($"/ledger/fiscal-periods/{result.FiscalPeriodId}", UriKind.Relative),
            response.Headers.Location);
    }

    [Fact]
    public async Task OpenFiscalPeriod_WithInvalidPeriod_ReturnsBadRequest()
    {
        await using var context = await LedgerApiTestContext.Create();

        var response = await context.Client.PostAsJsonAsync(
            "/ledger/fiscal-periods",
            new OpenFiscalPeriodCommand(
                Guid.NewGuid(),
                new DateOnly(2026, 12, 31),
                new DateOnly(2026, 1, 1)));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem.Status);
    }
}
