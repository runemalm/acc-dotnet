using ACC.Authority.Domain.Aggregates;

namespace ACC.Authority.Domain.Powers;

public sealed class RolePowerPolicy
{
    public bool Grants(Role role, Power power) =>
        role switch
        {
            Role.Owner => power is
                Power.AssignRole or
                Power.RevokeRole,
            _ => false
        };
}
