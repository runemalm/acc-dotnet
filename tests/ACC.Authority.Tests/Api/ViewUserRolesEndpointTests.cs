using System.Net;
using System.Net.Http.Json;
using ACC.Authority.Application.UseCases.AssignRole;
using ACC.Authority.Application.UseCases.ViewUserRoles;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Infrastructure.Endpoints;
using ACC.Authority.Tests.TestKit;
using ACC.Testing.Authentication;
using Xunit;

namespace ACC.Authority.Tests.Api;

public sealed class ViewUserRolesEndpointTests
{
    [Fact]
    public async Task ViewUserRoles_WithActiveRoles_ReturnsOk()
    {
        await using var context = await AuthorityApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var actorUserId = context.EstablishOwner(accountingSubjectId);
        var userId = Guid.NewGuid();
        context.RecognizeUser(userId);
        context.Client.AuthenticateAs(actorUserId);

        var assigned = await context.Client.PostAsJsonAsync(
            "/authority/assign-role",
            new AssignRoleRequest(
                userId,
                accountingSubjectId,
                Role.Owner));
        var assignedResult = await assigned.Content.ReadFromJsonAsync<AssignRoleResult>();
        context.Client.AuthenticateAs(userId);

        var response = await context.Client.GetAsync("/authority/roles");

        var result = await response.Content.ReadFromJsonAsync<ViewUserRolesResponse>();
        var role = Assert.Single(result!.Roles);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(assignedResult!.RoleAssignmentId, role.RoleAssignmentId);
        Assert.Equal(accountingSubjectId, role.AccountingSubjectId);
        Assert.Equal(nameof(Role.Owner), role.Role);
    }

    [Fact]
    public async Task ViewUserRoles_WithAnotherUsersRoles_ReturnsOnlyOwnRoles()
    {
        await using var context = await AuthorityApiTestContext.Create();
        var accountingSubjectId = Guid.NewGuid();
        var ownerUserId = context.EstablishOwner(accountingSubjectId);
        var otherUserId = Guid.NewGuid();
        var viewingUserId = Guid.NewGuid();
        context.RecognizeUser(otherUserId);
        context.RecognizeUser(viewingUserId);
        context.Client.AuthenticateAs(ownerUserId);
        await context.Client.PostAsJsonAsync(
            "/authority/assign-role",
            new AssignRoleRequest(
                otherUserId,
                accountingSubjectId,
                Role.Owner));
        context.Client.AuthenticateAs(viewingUserId);

        var response = await context.Client.GetAsync("/authority/roles");

        var result = await response.Content.ReadFromJsonAsync<ViewUserRolesResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(viewingUserId, result.UserId);
        Assert.Empty(result.Roles);
    }
}
