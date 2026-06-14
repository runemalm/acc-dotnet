using System.Net;
using System.Net.Http.Json;
using ACC.Authority.Application.UseCases.AssignRole;
using ACC.Authority.Application.UseCases.ViewUserRoles;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Tests.TestKit;
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

        var assigned = await context.Client.PostAsJsonAsync(
            "/authority/assign-role",
            new AssignRoleCommand(
                actorUserId,
                userId,
                accountingSubjectId,
                Role.Owner));
        var assignedResult = await assigned.Content.ReadFromJsonAsync<AssignRoleResult>();

        var response = await context.Client.GetAsync($"/authority/users/{userId}/roles");

        var result = await response.Content.ReadFromJsonAsync<ViewUserRolesResponse>();
        var role = Assert.Single(result!.Roles);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(assignedResult!.RoleAssignmentId, role.RoleAssignmentId);
        Assert.Equal(accountingSubjectId, role.AccountingSubjectId);
        Assert.Equal(nameof(Role.Owner), role.Role);
    }

    [Fact]
    public async Task ViewUserRoles_WithNoActiveRoles_ReturnsOk()
    {
        await using var context = await AuthorityApiTestContext.Create();
        var userId = Guid.NewGuid();

        var response = await context.Client.GetAsync($"/authority/users/{userId}/roles");

        var result = await response.Content.ReadFromJsonAsync<ViewUserRolesResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Empty(result.Roles);
    }
}
