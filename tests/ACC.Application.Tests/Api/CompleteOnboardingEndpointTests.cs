using System.Net;
using System.Net.Http.Json;
using ACC.AccountingSubject.Domain.Aggregates;
using ACC.Application.Application.UseCases.CompleteOnboarding;
using ACC.Application.Tests.TestKit;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.Application.Tests.Api;

public sealed class CompleteOnboardingEndpointTests
{
    [Fact]
    public async Task CompleteOnboarding_WithExistingUserAndTemplate_ReturnsCreated()
    {
        await using var context = await ApplicationApiTestContext.Create();
        var userId = Guid.NewGuid();
        const string templateId = "se:bas:k1:2018";
        context.RecognizeUser(userId);
        context.AddTemplate(templateId, "BAS 2018 för K1");

        var response = await context.Client.PostAsJsonAsync(
            "/onboarding/complete",
            Command(userId, templateId));

        var result = await response.Content.ReadFromJsonAsync<CompleteOnboardingResult>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.AccountingSubjectId);
        Assert.Equal(
            new Uri($"/accounting-subjects/{result.AccountingSubjectId}", UriKind.Relative),
            response.Headers.Location);
    }

    [Fact]
    public async Task CompleteOnboarding_WithUnknownTemplate_ReturnsBadRequest()
    {
        await using var context = await ApplicationApiTestContext.Create();
        var userId = Guid.NewGuid();
        context.RecognizeUser(userId);

        var response = await context.Client.PostAsJsonAsync(
            "/onboarding/complete",
            Command(userId, "unknown-template"));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains("template unknown-template is not recognized", problem.Detail);
    }

    private static CompleteOnboardingCommand Command(Guid userId, string templateId) =>
        new(
            userId,
            "Example Business",
            "198001011234",
            AccountingSubjectType.EnskildFirma,
            Country.Sweden,
            AccountingMethod.Cash,
            VatReportingPeriod.Quarterly,
            templateId);
}
