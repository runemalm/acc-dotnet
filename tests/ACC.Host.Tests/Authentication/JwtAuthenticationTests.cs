using System.Net;
using System.Net.Http.Headers;
using ACC.BuildingBlocks.Security;
using ACC.Host;
using ACC.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ACC.Host.Tests.Authentication;

public sealed class JwtAuthenticationTests
{
    private const string SigningKey =
        "acc-host-tests-signing-key-with-at-least-thirty-two-bytes";

    [Fact]
    public async Task JwtAuthentication_WithValidToken_ReturnsOk()
    {
        await using var context = await JwtTestContext.Create();
        var userId = Guid.NewGuid();
        var token = context.IssueToken(userId, SigningKey);

        var response = await context.GetProtected(token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(userId.ToString(), await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task JwtAuthentication_WithInvalidSignature_ReturnsUnauthorized()
    {
        await using var context = await JwtTestContext.Create();
        var token = context.IssueToken(
            Guid.NewGuid(),
            "different-test-signing-key-with-at-least-thirty-two-bytes");

        var response = await context.GetProtected(token);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed class JwtTestContext : IAsyncDisposable
    {
        private readonly WebApplication app;

        private JwtTestContext(WebApplication app, HttpClient client)
        {
            this.app = app;
            Client = client;
        }

        private HttpClient Client { get; }

        public static async Task<JwtTestContext> Create()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
            AddJwtConfiguration(builder.Configuration, SigningKey);
            builder.Services.AddHostAuthentication(builder.Configuration);

            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapGet("/protected", (HttpContext context) =>
                    context.User.GetRequiredUserId().ToString())
                .RequireAuthorization();
            await app.StartAsync();

            var address = app.Services
                .GetRequiredService<IServer>()
                .Features
                .Get<IServerAddressesFeature>()!
                .Addresses
                .Single();

            return new JwtTestContext(
                app,
                new HttpClient { BaseAddress = new Uri(address) });
        }

        public string IssueToken(Guid userId, string signingKey)
        {
            var configurationBuilder = new ConfigurationBuilder();
            AddJwtConfiguration(configurationBuilder, signingKey);
            var configuration = configurationBuilder.Build();

            return new JwtAuthenticationTokenIssuer(configuration)
                .Issue(userId, "user@example.com", DateTimeOffset.UtcNow)
                .AccessToken;
        }

        public Task<HttpResponseMessage> GetProtected(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return Client.SendAsync(request);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }

        private static void AddJwtConfiguration(
            IConfigurationBuilder configuration,
            string signingKey) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Identity:Jwt:Issuer"] = "ACC.Tests",
                ["Identity:Jwt:Audience"] = "ACC.Tests",
                ["Identity:Jwt:SigningKey"] = signingKey,
                ["Identity:Jwt:ExpiresMinutes"] = "60"
            });
    }
}
