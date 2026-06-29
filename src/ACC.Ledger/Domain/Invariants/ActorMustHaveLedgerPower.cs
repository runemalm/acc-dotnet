using ACC.BuildingBlocks.Domain;

namespace ACC.Ledger.Domain.Invariants;

public static class ActorMustHaveLedgerPower
{
    public static void Ensure(bool hasPower, Guid actorUserId, string act)
    {
        if (!hasPower)
        {
            throw new ActorMustHaveLedgerPowerViolation(actorUserId, act);
        }
    }
}

public sealed class ActorMustHaveLedgerPowerViolation(Guid actorUserId, string act)
    : InvariantViolationException($"User {actorUserId} must have power to {act}.");
