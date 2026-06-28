using ACC.BuildingBlocks.Authorization;

namespace ACC.Ledger.Domain.Invariants;

public static class ActorMustHaveLedgerPower
{
    public static void Ensure(bool hasPower, Guid actorUserId, string act)
    {
        if (!hasPower)
        {
            throw new AuthorizationDeniedException(
                $"User {actorUserId} must have power to {act}.");
        }
    }
}
