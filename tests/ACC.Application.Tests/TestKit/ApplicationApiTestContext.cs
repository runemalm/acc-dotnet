using ACC.AccountingSubject;
using ACC.Application;
using ACC.Authority;
using ACC.Authority.Application.Ports.Identity;
using ACC.ChartOfAccounts;
using ACC.ChartOfAccounts.Application.Ports.Templates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ACC.Application.Tests.TestKit;

public sealed class ApplicationApiTestContext : IAsyncDisposable
{
    private readonly WebApplication app;
    private readonly TestRecognizedUserPort recognizedUsers;
    private readonly TestChartOfAccountsTemplateCatalog templates;

    private ApplicationApiTestContext(
        WebApplication app,
        HttpClient client,
        TestRecognizedUserPort recognizedUsers,
        TestChartOfAccountsTemplateCatalog templates)
    {
        this.app = app;
        this.recognizedUsers = recognizedUsers;
        this.templates = templates;
        Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<ApplicationApiTestContext> Create()
    {
        var builder = WebApplication.CreateBuilder();
        var recognizedUsers = new TestRecognizedUserPort();
        var templates = new TestChartOfAccountsTemplateCatalog();

        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Services.AddAccountingSubjectApplication();
        builder.Services.AddAccountingSubjectMemoryPersistence();
        builder.Services.AddAuthorityApplication();
        builder.Services.AddAuthorityMemoryPersistence();
        builder.Services.AddChartOfAccountsApplication();
        builder.Services.AddChartOfAccountsMemoryPersistence();
        builder.Services.AddApplication();
        builder.Services.RemoveAll<IRecognizedUserPort>();
        builder.Services.RemoveAll<IChartOfAccountsTemplateCatalog>();
        builder.Services.AddSingleton<IRecognizedUserPort>(recognizedUsers);
        builder.Services.AddSingleton<IChartOfAccountsTemplateCatalog>(templates);

        var app = builder.Build();
        app.MapApplication();
        await app.StartAsync();

        var address = app.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()!
            .Addresses
            .Single();

        return new ApplicationApiTestContext(
            app,
            new HttpClient { BaseAddress = new Uri(address) },
            recognizedUsers,
            templates);
    }

    public void RecognizeUser(Guid userId) =>
        recognizedUsers.Recognize(userId);

    public void AddTemplate(string id, string name) =>
        templates.Add(new ChartOfAccountsTemplate(
            id,
            name,
            [
                new TemplateAccount("1000", "Assets"),
                new TemplateAccount("2000", "Equity and liabilities")
            ]));

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await app.DisposeAsync();
    }
}
