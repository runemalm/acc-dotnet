using ACC.Authority.Application.Ports.AccountingSubject;
using ACC.Authority.Application.Ports.Identity;
using ACC.Authority.Application.UseCases.EstablishInitialOwner;
using ACC.Testing.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ACC.Authority.Tests.TestKit;

public sealed class AuthorityApiTestContext : IAsyncDisposable
{
    private readonly WebApplication app;
    private readonly TestRecognizedUserPort recognizedUsers;
    private readonly TestRecognizedAccountingSubjectPort recognizedAccountingSubjects;

    private AuthorityApiTestContext(
        WebApplication app,
        HttpClient client,
        TestRecognizedUserPort recognizedUsers,
        TestRecognizedAccountingSubjectPort recognizedAccountingSubjects)
    {
        this.app = app;
        this.recognizedUsers = recognizedUsers;
        this.recognizedAccountingSubjects = recognizedAccountingSubjects;
        Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<AuthorityApiTestContext> Create()
    {
        var builder = WebApplication.CreateBuilder();
        var recognizedUsers = new TestRecognizedUserPort();
        var recognizedAccountingSubjects = new TestRecognizedAccountingSubjectPort();

        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Services.AddAuthorityApplication();
        builder.Services.AddAuthorityMemoryPersistence();
        builder.Services.RemoveAll<IRecognizedUserPort>();
        builder.Services.RemoveAll<IRecognizedAccountingSubjectPort>();
        builder.Services.AddSingleton<IRecognizedUserPort>(recognizedUsers);
        builder.Services.AddSingleton<IRecognizedAccountingSubjectPort>(recognizedAccountingSubjects);
        builder.Services.AddTestAuthentication();

        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapAuthority();

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

        var context = new AuthorityApiTestContext(
            app,
            client,
            recognizedUsers,
            recognizedAccountingSubjects);
        context.Client.AuthenticateAs(Guid.NewGuid());

        return context;
    }

    public void RecognizeUser(Guid userId) =>
        recognizedUsers.Recognize(userId);

    public void RecognizeAccountingSubject(Guid accountingSubjectId) =>
        recognizedAccountingSubjects.Recognize(accountingSubjectId);

    public Guid EstablishOwner(Guid accountingSubjectId)
    {
        using var scope = app.Services.CreateScope();
        var ownerUserId = Guid.NewGuid();
        var handler = scope.ServiceProvider.GetRequiredService<EstablishInitialOwnerHandler>();

        RecognizeUser(ownerUserId);
        RecognizeAccountingSubject(accountingSubjectId);

        handler.Handle(
            new EstablishInitialOwnerCommand(ownerUserId, accountingSubjectId),
            DateTimeOffset.UtcNow);

        return ownerUserId;
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await app.DisposeAsync();
    }

    private sealed class TestRecognizedUserPort : IRecognizedUserPort
    {
        private readonly HashSet<Guid> userIds = [];

        public bool IsRecognizedUser(Guid userId) =>
            userIds.Contains(userId);

        public void Recognize(Guid userId) =>
            userIds.Add(userId);
    }

    private sealed class TestRecognizedAccountingSubjectPort : IRecognizedAccountingSubjectPort
    {
        private readonly HashSet<Guid> accountingSubjectIds = [];

        public bool IsRecognizedAccountingSubject(Guid accountingSubjectId) =>
            accountingSubjectIds.Contains(accountingSubjectId);

        public void Recognize(Guid accountingSubjectId) =>
            accountingSubjectIds.Add(accountingSubjectId);
    }
}
