using System.Net;
using System.Net.Http.Json;
using ACC.Authority.Application.UseCases.AssignRole;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Tests.TestKit;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.Authority.Tests.Api;

public sealed class AssignRoleEndpointTests
{
    [Fact]
    public async Task AssignRole_WithAuthorizedActor_ReturnsCreated()
    {
        await using var context = await AuthorityApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = context.EstablishOwner(accountingSubjectId);
        var userId = Guid.NewGuid();
        context.RecognizeUser(userId);

        var response = await context.Client.PostAsJsonAsync(
            "/authority/assign-role",
            new AssignRoleCommand(
                actorUserId,
                userId,
                accountingSubjectId,
                Role.Owner));

        var result = await response.Content.ReadFromJsonAsync<AssignRoleResult>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.RoleAssignmentId);
        Assert.Equal(
            new Uri($"/authority/role-assignments/{result.RoleAssignmentId}", UriKind.Relative),
            response.Headers.Location);
    }

    [Fact]
    public async Task AssignRole_WithActorWithoutAssignRolePower_ReturnsBadRequest()
    {
        await using var context = await AuthorityApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        context.RecognizeAccountingSubject(accountingSubjectId);
        context.RecognizeUser(actorUserId);
        context.RecognizeUser(userId);

        var response = await context.Client.PostAsJsonAsync(
            "/authority/assign-role",
            new AssignRoleCommand(
                actorUserId,
                userId,
                accountingSubjectId,
                Role.Owner));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem.Status);
        Assert.Contains("must have AssignRole power", problem.Detail);
    }
}
