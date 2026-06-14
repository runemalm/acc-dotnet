using ACC.Authority.Application.UseCases.AssignRole;
using ACC.Authority.Application.UseCases.RevokeRole;
using ACC.Authority.Application.UseCases.ViewUserRoles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ACC.Authority.Infrastructure.Endpoints;

internal static class AuthorityEndpoints
{
    public static IEndpointRouteBuilder MapAuthorityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/assign-role", (
            AssignRoleCommand command,
            AssignRoleHandler handler) =>
        {
            try
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Created($"/authority/role-assignments/{result.RoleAssignmentId}", result);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                return BadRequest(exception);
            }
        })
        .WithName("AssignRole")
        .WithTags("Authority")
        .WithSummary("Assign role")
        .WithDescription("Assigns a role to a recognized user for a recognized accounting subject.")
        .Produces<AssignRoleResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/revoke-role", (
            RevokeRoleCommand command,
            RevokeRoleHandler handler) =>
        {
            try
            {
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Ok(result);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                return BadRequest(exception);
            }
        })
        .WithName("RevokeRole")
        .WithTags("Authority")
        .WithSummary("Revoke role")
        .WithDescription("Revokes an active role assignment.")
        .Produces<RevokeRoleResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        endpoints.MapGet("/users/{userId:guid}/roles", (
            Guid userId,
            ViewUserRolesHandler handler) =>
        {
            var result = handler.Handle(new ViewUserRolesQuery(userId));

            return Results.Ok(result);
        })
        .WithName("ViewUserRoles")
        .WithTags("Authority")
        .WithSummary("View user roles")
        .WithDescription("Returns active role assignments for a user.")
        .Produces<ViewUserRolesResponse>(StatusCodes.Status200OK);

        return endpoints;
    }

    private static IResult BadRequest(Exception exception) =>
        Results.Problem(
            detail: exception.Message,
            statusCode: StatusCodes.Status400BadRequest);
}
