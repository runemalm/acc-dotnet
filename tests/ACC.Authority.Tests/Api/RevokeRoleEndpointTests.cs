using System.Net;
using System.Net.Http.Json;
using ACC.Authority.Application.UseCases.AssignRole;
using ACC.Authority.Application.UseCases.RevokeRole;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Tests.TestKit;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.Authority.Tests.Api;

public sealed class RevokeRoleEndpointTests
{
    [Fact]
    public async Task RevokeRole_WithActiveRoleAssignment_ReturnsOk()
    {
        await using var context = await AuthorityApiTestContext.Create();
        var assigned = await AssignOwnerRole(context);

        var response = await context.Client.PostAsJsonAsync(
            "/authority/revoke-role",
            new RevokeRoleCommand(
                assigned.ActorUserId,
                assigned.RoleAssignmentId));

        var result = await response.Content.ReadFromJsonAsync<RevokeRoleResult>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RevokeRole_WithRevokedRoleAssignment_ReturnsBadRequest()
    {
        await using var context = await AuthorityApiTestContext.Create();
        var assigned = await AssignOwnerRole(context);

        await context.Client.PostAsJsonAsync(
            "/authority/revoke-role",
            new RevokeRoleCommand(
                assigned.ActorUserId,
                assigned.RoleAssignmentId));

        var response = await context.Client.PostAsJsonAsync(
            "/authority/revoke-role",
            new RevokeRoleCommand(
                assigned.ActorUserId,
                assigned.RoleAssignmentId));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem.Status);
        Assert.Equal("A role assignment must be active before it can be revoked.", problem.Detail);
    }

    private static async Task<AssignedRole> AssignOwnerRole(AuthorityApiTestContext context)
    {
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

        return new AssignedRole(result!.RoleAssignmentId, actorUserId);
    }

    private sealed record AssignedRole(Guid RoleAssignmentId, Guid ActorUserId);
}
