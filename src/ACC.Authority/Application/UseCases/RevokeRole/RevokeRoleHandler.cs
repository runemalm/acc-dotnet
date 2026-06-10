namespace ACC.Authority.Application.UseCases.RevokeRole;

public sealed class RevokeRoleHandler
{
    public RevokeRoleResult Handle(RevokeRoleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        throw new NotImplementedException("Revoke role has not been implemented yet.");
    }
}
