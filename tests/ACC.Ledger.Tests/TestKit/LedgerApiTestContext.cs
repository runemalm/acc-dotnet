using ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;
using ACC.ChartOfAccounts.Infrastructure.ReadModels.ChartOfAccounts;
using ACC.Ledger.Application.UseCases.OpenFiscalPeriod;
using ACC.Ledger.Application.Ports.Authority;
using ACC.Testing.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ACC.Ledger.Tests.TestKit;

public sealed class LedgerApiTestContext : IAsyncDisposable
{
    private readonly WebApplication app;
    private readonly InMemoryAccountStore accountStore;
    private readonly TestLedgerAuthorityPort authority;

    private LedgerApiTestContext(
        WebApplication app,
        HttpClient client,
        InMemoryAccountStore accountStore,
        TestLedgerAuthorityPort authority)
    {
        this.app = app;
        this.accountStore = accountStore;
        this.authority = authority;
        Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<LedgerApiTestContext> Create()
    {
        var builder = WebApplication.CreateBuilder();
        var accountStore = new InMemoryAccountStore();
        var authority = new TestLedgerAuthorityPort(grantsAllByDefault: true);

        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Services.AddLedgerApplication();
        builder.Services.AddLedgerMemoryPersistence();
        builder.Services.AddSingleton<IAccountStore>(accountStore);
        builder.Services.RemoveAll<ILedgerAuthorityPort>();
        builder.Services.AddSingleton<ILedgerAuthorityPort>(authority);
        builder.Services.AddTestAuthentication();

        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();
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

        var context = new LedgerApiTestContext(app, client, accountStore, authority);
        context.Client.AuthenticateAs(Guid.NewGuid());

        return context;
    }

    public void OpenFiscalPeriod(Guid accountingSubjectId, DateOnly startsOn, DateOnly endsOn)
    {
        using var scope = app.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OpenFiscalPeriodHandler>();

        handler.Handle(
            new OpenFiscalPeriodCommand(
                Guid.NewGuid(),
                accountingSubjectId,
                startsOn,
                endsOn),
            DateTimeOffset.UtcNow);
    }

    public void MakeAccountsActive(Guid accountingSubjectId, params string[] accountNumbers)
    {
        foreach (var accountNumber in accountNumbers)
        {
            accountStore.Save(new AccountView(
                Guid.NewGuid(),
                accountingSubjectId,
                accountNumber,
                accountNumber,
                IsActive: true));
        }
    }

    public void DenyViewingJournalEntries(Guid actorUserId, Guid accountingSubjectId) =>
        authority.DenyViewing(actorUserId, accountingSubjectId);

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await app.DisposeAsync();
    }

}
