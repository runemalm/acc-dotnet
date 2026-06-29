using System.Net;
using System.Net.Http.Json;
using ACC.AccountingSubject.Domain.Aggregates;
using ACC.Application.Application.UseCases.CompleteOnboarding;
using ACC.Application.Infrastructure.Endpoints;
using ACC.Application.Tests.TestKit;
using ACC.Testing.Authentication;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.Application.Tests.Api;

public sealed class CompleteOnboardingEndpointTests
{
    [Fact]
    public async Task CompleteOnboarding_WithIncompleteRequest_ReturnsUnprocessableEntity()
    {
        await using var context = await ApplicationApiTestContext.Create();

        var response = await context.Client.PostAsJsonAsync(
            "/onboarding/complete",
            Request(string.Empty));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, problem.Status);
    }

    [Fact]
    public async Task CompleteOnboarding_WithExistingUserAndTemplate_ReturnsCreated()
    {
        await using var context = await ApplicationApiTestContext.Create();
        var userId = Guid.NewGuid();
        const string templateId = "se:bas:k1:2018";
        context.RecognizeUser(userId);
        context.AddTemplate(templateId, "BAS 2018 för K1");
        context.Client.AuthenticateAs(userId);

        var response = await context.Client.PostAsJsonAsync(
            "/onboarding/complete",
            Request(templateId));

        var result = await response.Content.ReadFromJsonAsync<CompleteOnboardingResult>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.AccountingSubjectId);
        Assert.Equal(
            new Uri($"/accounting-subjects/{result.AccountingSubjectId}", UriKind.Relative),
            response.Headers.Location);
    }

    [Fact]
    public async Task CompleteOnboarding_WithUnknownTemplate_ReturnsNotFound()
    {
        await using var context = await ApplicationApiTestContext.Create();
        var userId = Guid.NewGuid();
        context.RecognizeUser(userId);
        context.Client.AuthenticateAs(userId);

        var response = await context.Client.PostAsJsonAsync(
            "/onboarding/complete",
            Request("unknown-template"));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains("template unknown-template is not recognized", problem.Detail);
    }

    [Fact]
    public async Task CompleteOnboarding_WithoutAuthentication_ReturnsUnauthorized()
    {
        await using var context = await ApplicationApiTestContext.Create();
        context.Client.SignOut();

        var response = await context.Client.PostAsJsonAsync(
            "/onboarding/complete",
            Request("se:bas:k1:2018"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static CompleteOnboardingRequest Request(string templateId) =>
        new(
            "Example Business",
            "198001011234",
            AccountingSubjectType.EnskildFirma,
            Country.Sweden,
            AccountingMethod.Cash,
            VatReportingPeriod.Quarterly,
            templateId);
}
