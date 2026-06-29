using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.Failures;
using ACC.Host.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace ACC.Host.Tests.Errors;

public sealed class UnhandledExceptionHandlingTests
{
    private const string ExceptionMessage = "Sensitive internal failure.";

    [Fact]
    public async Task UnhandledException_InProduction_ReturnsRedactedProblemDetails()
    {
        await using var context = await ErrorTestContext.Create(Environments.Production);

        var response = await context.Client.GetAsync("/throw");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(problem);
        Assert.Equal("An unexpected error occurred.", problem.Title);
        Assert.Null(problem.Detail);
        Assert.True(problem.Extensions.ContainsKey("traceId"));
        Assert.False(problem.Extensions.ContainsKey("stackTrace"));
    }

    [Fact]
    public async Task ArgumentException_InProduction_ReturnsInternalServerError()
    {
        await using var context = await ErrorTestContext.Create(Environments.Production);

        var response = await context.Client.GetAsync("/throw-argument");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("An unexpected error occurred.", problem.Title);
        Assert.Null(problem.Detail);
    }

    [Fact]
    public async Task ApplicationValidationException_ReturnsUnprocessableEntity()
    {
        await using var context = await ErrorTestContext.Create(Environments.Production);

        var response = await context.Client.GetAsync("/throw-application-validation");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal(ExceptionMessage, problem.Detail);
    }

    [Fact]
    public async Task WrongExpectedStreamVersionException_DoesNotExposeStreamDetails()
    {
        await using var context = await ErrorTestContext.Create(Environments.Production);

        var response = await context.Client.GetAsync("/throw-stream-version");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal(
            "The operation conflicts with changes made since its state was read.",
            problem.Detail);
        Assert.DoesNotContain("sensitive-stream", problem.Detail);
    }

    [Fact]
    public async Task UnhandledException_InDevelopment_ReturnsDiagnosticProblemDetails()
    {
        await using var context = await ErrorTestContext.Create(Environments.Development);

        var response = await context.Client.GetAsync("/throw");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal(ExceptionMessage, problem.Detail);
        var stackTrace = Assert.IsType<JsonElement>(problem.Extensions["stackTrace"]);
        Assert.Contains(nameof(InvalidOperationException), stackTrace.GetString());
        Assert.Contains(ExceptionMessage, stackTrace.GetString());
    }

    [Fact]
    public async Task Swagger_DocumentsInternalServerErrorForEveryOperation()
    {
        await using var context = await SwaggerTestContext.Create();

        var document = await context.Client.GetFromJsonAsync<JsonElement>(
            "/swagger/v1/swagger.json");
        var response = document
            .GetProperty("paths")
            .GetProperty("/ok")
            .GetProperty("get")
            .GetProperty("responses")
            .GetProperty("500");

        Assert.Equal(
            "An unexpected server error occurred.",
            response.GetProperty("description").GetString());
        Assert.True(response
            .GetProperty("content")
            .TryGetProperty("application/problem+json", out _));
    }

    private sealed class ErrorTestContext : TestAppContext
    {
        private ErrorTestContext(WebApplication app, HttpClient client)
            : base(app, client)
        {
        }

        public static async Task<ErrorTestContext> Create(string environmentName)
        {
            var builder = CreateBuilder(environmentName);
            builder.Services.AddHostErrorHandling();

            var app = builder.Build();
            app.UseExceptionHandler();
            app.MapGet("/throw", IResult () =>
                throw new InvalidOperationException(ExceptionMessage));
            app.MapGet("/throw-argument", IResult () =>
                throw new ArgumentException(ExceptionMessage));
            app.MapGet("/throw-application-validation", IResult () =>
                throw new ApplicationValidationException(ExceptionMessage));
            app.MapGet("/throw-stream-version", IResult () =>
                throw new WrongExpectedStreamVersionException(
                    StreamId.For("sensitive-stream", Guid.NewGuid()),
                    expectedVersion: 1,
                    actualVersion: 2));

            var client = await Start(app);
            return new ErrorTestContext(app, client);
        }
    }

    private sealed class SwaggerTestContext : TestAppContext
    {
        private SwaggerTestContext(WebApplication app, HttpClient client)
            : base(app, client)
        {
        }

        public static async Task<SwaggerTestContext> Create()
        {
            var builder = CreateBuilder(Environments.Development);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
                options.OperationFilter<InternalServerErrorOperationFilter>());

            var app = builder.Build();
            app.UseSwagger();
            app.MapGet("/ok", () => Results.Ok());

            var client = await Start(app);
            return new SwaggerTestContext(app, client);
        }
    }

    private abstract class TestAppContext : IAsyncDisposable
    {
        private readonly WebApplication app;

        protected TestAppContext(WebApplication app, HttpClient client)
        {
            this.app = app;
            Client = client;
        }

        public HttpClient Client { get; }

        protected static WebApplicationBuilder CreateBuilder(string environmentName)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = environmentName
            });
            builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

            return builder;
        }

        protected static async Task<HttpClient> Start(WebApplication app)
        {
            await app.StartAsync();

            var address = app.Services
                .GetRequiredService<IServer>()
                .Features
                .Get<IServerAddressesFeature>()!
                .Addresses
                .Single();

            return new HttpClient { BaseAddress = new Uri(address) };
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }
    }
}
