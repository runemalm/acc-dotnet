using ACC.Authority.Application.UseCases.AssignRole;
using ACC.Authority.Application.UseCases.RevokeRole;
using ACC.Authority.Application.UseCases.ViewUserRoles;
using ACC.Authority.Domain.Aggregates;
using ACC.BuildingBlocks.Authorization;
using ACC.BuildingBlocks.Failures;
using ACC.BuildingBlocks.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ACC.Authority.Infrastructure.Endpoints;

internal static class AuthorityEndpoints
{
    public static IEndpointRouteBuilder MapAuthorityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/assign-role", (
            AssignRoleRequest request,
            HttpContext context,
            AssignRoleHandler handler) =>
        {
            try
            {
                var command = new AssignRoleCommand(
                    context.User.GetRequiredUserId(),
                    request.UserId,
                    request.AccountingSubjectId,
                    request.Role);
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Created($"/authority/role-assignments/{result.RoleAssignmentId}", result);
            }
            catch (AuthorizationDeniedException exception)
            {
                return Forbidden(exception);
            }
            catch (ResourceNotFoundException exception)
            {
                return Problem(exception, StatusCodes.Status404NotFound);
            }
            catch (StateConflictException exception)
            {
                return Problem(exception, StatusCodes.Status409Conflict);
            }
            catch (Exception exception) when (exception is SemanticViolationException or ArgumentException)
            {
                return Problem(exception, StatusCodes.Status422UnprocessableEntity);
            }
        })
        .WithName("AssignRole")
        .WithTags("Authority")
        .WithSummary("Assign role")
        .WithDescription("Assigns a role to a recognized user for a recognized accounting subject.")
        .Produces<AssignRoleResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        endpoints.MapPost("/revoke-role", (
            RevokeRoleRequest request,
            HttpContext context,
            RevokeRoleHandler handler) =>
        {
            try
            {
                var command = new RevokeRoleCommand(
                    context.User.GetRequiredUserId(),
                    request.RoleAssignmentId);
                var result = handler.Handle(command, DateTimeOffset.UtcNow);

                return Results.Ok(result);
            }
            catch (AuthorizationDeniedException exception)
            {
                return Forbidden(exception);
            }
            catch (ResourceNotFoundException exception)
            {
                return Problem(exception, StatusCodes.Status404NotFound);
            }
            catch (StateConflictException exception)
            {
                return Problem(exception, StatusCodes.Status409Conflict);
            }
            catch (Exception exception) when (exception is SemanticViolationException or ArgumentException)
            {
                return Problem(exception, StatusCodes.Status422UnprocessableEntity);
            }
        })
        .WithName("RevokeRole")
        .WithTags("Authority")
        .WithSummary("Revoke role")
        .WithDescription("Revokes an active role assignment.")
        .Produces<RevokeRoleResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        endpoints.MapGet("/roles", (
            HttpContext context,
            ViewUserRolesHandler handler) =>
        {
            var result = handler.Handle(new ViewUserRolesQuery(
                context.User.GetRequiredUserId()));

            return Results.Ok(result);
        })
        .WithName("ViewUserRoles")
        .WithTags("Authority")
        .WithSummary("View user roles")
        .WithDescription("Returns the authenticated user's active role assignments.")
        .Produces<ViewUserRolesResponse>(StatusCodes.Status200OK);

        return endpoints;
    }

    private static IResult Problem(Exception exception, int statusCode) =>
        Results.Problem(
            detail: exception.Message,
            statusCode: statusCode);

    private static IResult Forbidden(AuthorizationDeniedException exception) =>
        Results.Problem(
            detail: exception.Message,
            statusCode: StatusCodes.Status403Forbidden);
}

public sealed record AssignRoleRequest(
    Guid UserId,
    Guid AccountingSubjectId,
    Role Role);

public sealed record RevokeRoleRequest(Guid RoleAssignmentId);
