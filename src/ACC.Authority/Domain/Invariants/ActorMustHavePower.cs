using ACC.Authority.Domain.Powers;
using ACC.BuildingBlocks.Domain;

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
            throw new ActorMustHavePowerViolation(
                actorUserId,
                accountingSubjectId,
                power);
        }
    }
}

public sealed class ActorMustHavePowerViolation(
    Guid actorUserId,
    Guid accountingSubjectId,
    Power power)
    : InvariantViolationException(
        $"User {actorUserId} must have {power} power for accounting subject {accountingSubjectId}.");
