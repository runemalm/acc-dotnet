using ACC.Authority.Domain.Powers;

namespace ACC.Authority.Domain.Invariants;

public static class ActorMustHavePower
{
    public static void Ensure(
        bool hasPower,
        Guid actorUserId,
        Guid accountingSubjectId,
        Power power)
    {
        if (!hasPower)
        {
            throw new InvalidOperationException(
                $"User {actorUserId} must have {power} power for accounting subject {accountingSubjectId}.");
        }
    }
}
