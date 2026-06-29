using System.Net;
using System.Net.Http.Json;
using ACC.Authority.Application.UseCases.AssignRole;
using ACC.Authority.Application.UseCases.RevokeRole;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Infrastructure.Endpoints;
using ACC.Authority.Tests.TestKit;
using ACC.Testing.Authentication;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ACC.Authority.Tests.Api;

public sealed class RevokeRoleEndpointTests
{
    [Fact]
    public async Task RevokeRole_WithMissingRoleAssignment_ReturnsNotFound()
    {
        await using var context = await AuthorityApiTestContext.Create();
        var actorUserId = Guid.NewGuid();
        context.RecognizeUser(actorUserId);
        context.Client.AuthenticateAs(actorUserId);

        var response = await context.Client.PostAsJsonAsync(
            "/authority/revoke-role",
            new RevokeRoleRequest(Guid.NewGuid()));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.NotFound, problem.Status);
    }

    [Fact]
    public async Task RevokeRole_WithActiveRoleAssignment_ReturnsOk()
    {
        await using var context = await AuthorityApiTestContext.Create();
        var assigned = await AssignOwnerRole(context);
        context.Client.AuthenticateAs(assigned.ActorUserId);

        var response = await context.Client.PostAsJsonAsync(
            "/authority/revoke-role",
            new RevokeRoleRequest(assigned.RoleAssignmentId));

        var result = await response.Content.ReadFromJsonAsync<RevokeRoleResult>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RevokeRole_WithRevokedRoleAssignment_ReturnsConflict()
    {
        await using var context = await AuthorityApiTestContext.Create();
        var assigned = await AssignOwnerRole(context);
        context.Client.AuthenticateAs(assigned.ActorUserId);

        await context.Client.PostAsJsonAsync(
            "/authority/revoke-role",
            new RevokeRoleRequest(assigned.RoleAssignmentId));

        var response = await context.Client.PostAsJsonAsync(
            "/authority/revoke-role",
            new RevokeRoleRequest(assigned.RoleAssignmentId));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.Conflict, problem.Status);
        Assert.Equal("A role assignment must be active before it can be revoked.", problem.Detail);
    }

    private static async Task<AssignedRole> AssignOwnerRole(AuthorityApiTestContext context)
    {
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = context.EstablishOwner(accountingSubjectId);
        var userId = Guid.NewGuid();
        context.RecognizeUser(userId);
        context.Client.AuthenticateAs(actorUserId);

        var response = await context.Client.PostAsJsonAsync(
            "/authority/assign-role",
            new AssignRoleRequest(
                userId,
                accountingSubjectId,
                Role.Owner));

        var result = await response.Content.ReadFromJsonAsync<AssignRoleResult>();

        return new AssignedRole(result!.RoleAssignmentId, actorUserId);
    }

    private sealed record AssignedRole(Guid RoleAssignmentId, Guid ActorUserId);
}
