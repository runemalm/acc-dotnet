using ACC.BuildingBlocks.Domain;

namespace ACC.ChartOfAccounts.Domain.Invariants;

public static class ActorMustHaveChartOfAccountsPower
{
    public static void Ensure(bool hasPower, Guid actorUserId, string act)
    {
        if (!hasPower)
        {
            throw new ActorMustHaveChartOfAccountsPowerViolation(actorUserId, act);
        }
    }
}

public sealed class ActorMustHaveChartOfAccountsPowerViolation(Guid actorUserId, string act)
    : InvariantViolationException(
        $"User {actorUserId} must have power to {act}.");
