using ACC.ChartOfAccounts.Application.Ports.AccountingSubject;
using ACC.ChartOfAccounts.Application.Ports.Authority;
using ACC.ChartOfAccounts.Application.Ports.Templates;
using ACC.ChartOfAccounts.Application.UseCases.AdoptChartOfAccounts;
using ACC.ChartOfAccounts.Application.UseCases.DeactivateAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ACC.ChartOfAccounts.Tests.TestKit;

public sealed class ChartOfAccountsApiTestContext : IAsyncDisposable
{
    private readonly WebApplication app;
    private readonly TestChartOfAccountsTemplateCatalog templates;
    private readonly TestRecognizedAccountingSubjectPort recognizedAccountingSubjects;
    private readonly TestChartOfAccountsAuthorityPort authority;

    private ChartOfAccountsApiTestContext(
        WebApplication app,
        HttpClient client,
        TestChartOfAccountsTemplateCatalog templates,
        TestRecognizedAccountingSubjectPort recognizedAccountingSubjects,
        TestChartOfAccountsAuthorityPort authority)
    {
        this.app = app;
        this.templates = templates;
        this.recognizedAccountingSubjects = recognizedAccountingSubjects;
        this.authority = authority;
        Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<ChartOfAccountsApiTestContext> Create()
    {
        var builder = WebApplication.CreateBuilder();
        var templates = new TestChartOfAccountsTemplateCatalog();
        var recognizedAccountingSubjects = new TestRecognizedAccountingSubjectPort();
        var authority = new TestChartOfAccountsAuthorityPort();

        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Services.AddChartOfAccountsApplication();
        builder.Services.AddChartOfAccountsMemoryPersistence();
        builder.Services.RemoveAll<IChartOfAccountsTemplateCatalog>();
        builder.Services.RemoveAll<IRecognizedAccountingSubjectPort>();
        builder.Services.RemoveAll<IChartOfAccountsAuthorityPort>();
        builder.Services.AddSingleton<IChartOfAccountsTemplateCatalog>(templates);
        builder.Services.AddSingleton<IRecognizedAccountingSubjectPort>(recognizedAccountingSubjects);
        builder.Services.AddSingleton<IChartOfAccountsAuthorityPort>(authority);

        var app = builder.Build();
        app.MapChartOfAccounts();
        await app.StartAsync();

        var address = app.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()!
            .Addresses
            .Single();

        return new ChartOfAccountsApiTestContext(
            app,
            new HttpClient { BaseAddress = new Uri(address) },
            templates,
            recognizedAccountingSubjects,
            authority);
    }

    public void AddTemplate(
        string id = "test-template",
        string name = "Test chart of accounts") =>
        templates.Add(new ChartOfAccountsTemplate(
            id,
            name,
            [
                new TemplateAccount("1000", "Assets"),
                new TemplateAccount("2000", "Equity and liabilities")
            ]));

    public void RecognizeAccountingSubject(Guid accountingSubjectId) =>
        recognizedAccountingSubjects.Recognize(accountingSubjectId);

    public void AllowAdoption(Guid actorUserId, Guid accountingSubjectId) =>
        authority.AllowAdoption(actorUserId, accountingSubjectId);

    public void AllowManagement(Guid actorUserId, Guid accountingSubjectId) =>
        authority.AllowManagement(actorUserId, accountingSubjectId);

    public Guid AdoptChart(Guid accountingSubjectId, Guid actorUserId)
    {
        AddTemplate();
        RecognizeAccountingSubject(accountingSubjectId);
        AllowAdoption(actorUserId, accountingSubjectId);
        AllowManagement(actorUserId, accountingSubjectId);

        using var scope = app.Services.CreateScope();
        return scope.ServiceProvider
            .GetRequiredService<AdoptChartOfAccountsHandler>()
            .Handle(
                new AdoptChartOfAccountsCommand(
                    actorUserId,
                    accountingSubjectId,
                    "test-template"),
                DateTimeOffset.UtcNow)
            .ChartOfAccountsId;
    }

    public void DeactivateAccount(Guid chartOfAccountsId, Guid actorUserId, string accountNumber)
    {
        using var scope = app.Services.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<DeactivateAccountHandler>()
            .Handle(
                new DeactivateAccountCommand(actorUserId, chartOfAccountsId, accountNumber),
                DateTimeOffset.UtcNow);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await app.DisposeAsync();
    }
}
