namespace ACC.Authority.Application.UseCases.GetUserRoles;

public sealed class GetUserRolesHandler
{
    public GetUserRolesResponse Handle(GetUserRolesQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        throw new NotImplementedException("Get user roles has not been implemented yet.");
    }
}
