namespace ACC.ChartOfAccounts.Domain.Invariants;

public static class ActorMustHaveChartOfAccountsPower
{
    public static void Ensure(bool hasPower, Guid actorUserId, string act)
    {
        if (!hasPower)
        {
            throw new InvalidOperationException(
                $"User {actorUserId} must have power to {act}.");
        }
    }
}
