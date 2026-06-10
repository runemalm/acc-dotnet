namespace ACC.Authority.Application.UseCases.AssignRole;

public sealed class AssignRoleHandler
{
    public AssignRoleResult Handle(AssignRoleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        throw new NotImplementedException("Assign role has not been implemented yet.");
    }
}
