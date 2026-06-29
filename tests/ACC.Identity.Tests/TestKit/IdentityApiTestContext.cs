using ACC.Identity.Application.Ports.Communication;
using ACC.Identity.Application.Ports.Security;
using ACC.BuildingBlocks.AspNetCore.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace ACC.Identity.Tests.TestKit;

public sealed class IdentityApiTestContext : IAsyncDisposable
{
    private readonly WebApplication app;

    private IdentityApiTestContext(
        WebApplication app,
        HttpClient client,
        RecordingIdentityEmailSender emailSender)
    {
        this.app = app;
        Client = client;
        EmailSender = emailSender;
    }

    public HttpClient Client { get; }

    public RecordingIdentityEmailSender EmailSender { get; }

    public static async Task<IdentityApiTestContext> Create()
    {
        var context = new IdentityApiTestContextBuilder();

        return await context.Build();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await app.DisposeAsync();
    }

    private sealed class IdentityApiTestContextBuilder
    {
        public async Task<IdentityApiTestContext> Build()
        {
            var builder = WebApplication.CreateBuilder();
            var emailSender = new RecordingIdentityEmailSender();

            builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
            builder.Services.AddIdentityApplication();
            builder.Services.AddIdentityMemoryPersistence();
            builder.Services.AddSingleton<IIdentityEmailSender>(emailSender);
            builder.Services.AddSingleton<IAuthenticationTokenIssuer, TestAuthenticationTokenIssuer>();
            builder.Services.AddExpectedExceptionHandling();

            var app = builder.Build();

            app.UseExceptionHandler();
            app.MapIdentity();

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

            return new IdentityApiTestContext(app, client, emailSender);
        }
    }

    public sealed class RecordingIdentityEmailSender : IIdentityEmailSender
    {
        private readonly List<VerificationEmail> sentVerificationEmails = [];

        public IReadOnlyCollection<VerificationEmail> SentVerificationEmails =>
            sentVerificationEmails.ToArray();

        public void SendVerificationEmail(
            string email,
            string verificationToken,
            DateTimeOffset expiresAt)
        {
            sentVerificationEmails.Add(new VerificationEmail(
                email,
                verificationToken,
                expiresAt));
        }
    }

    public sealed record VerificationEmail(
        string Email,
        string VerificationToken,
        DateTimeOffset ExpiresAt);

    private sealed class TestAuthenticationTokenIssuer : IAuthenticationTokenIssuer
    {
        public AuthenticationToken Issue(Guid userId, string email, DateTimeOffset issuedAt) =>
            new("access-token", issuedAt.AddMinutes(60));
    }
}
