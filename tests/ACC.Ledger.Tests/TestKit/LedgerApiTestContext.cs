using ACC.Ledger.Application.UseCases.OpenFiscalPeriod;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace ACC.Ledger.Tests.TestKit;

public sealed class LedgerApiTestContext : IAsyncDisposable
{
    private readonly WebApplication app;

    private LedgerApiTestContext(WebApplication app, HttpClient client)
    {
        this.app = app;
        Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<LedgerApiTestContext> Create()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Services.AddLedgerApplication();
        builder.Services.AddLedgerMemoryPersistence();

        var app = builder.Build();

        app.MapLedger();

        await app.StartAsync();

        var address = app.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()!
            .Addresses
            .Single();

        var client = new HttpClient
        {
            BaseAddress = new Uri(address)
        };

        return new LedgerApiTestContext(app, client);
    }

    public void OpenFiscalPeriod(Guid accountingSubjectId, DateOnly startsOn, DateOnly endsOn)
    {
        using var scope = app.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OpenFiscalPeriodHandler>();

        handler.Handle(
            new OpenFiscalPeriodCommand(accountingSubjectId, startsOn, endsOn),
            DateTimeOffset.UtcNow);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await app.DisposeAsync();
    }
}
