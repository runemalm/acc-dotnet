using ACC.BuildingBlocks.Domain;

namespace ACC.Authority.Domain.Invariants;

public static class AccountingSubjectMustBeRecognizedForAuthority
{
    public static void Ensure(bool isRecognized, Guid accountingSubjectId)
    {
        if (!isRecognized)
        {
            throw new AccountingSubjectMustBeRecognizedForAuthorityViolation(accountingSubjectId);
        }
    }
}

public sealed class AccountingSubjectMustBeRecognizedForAuthorityViolation(Guid accountingSubjectId)
    : InvariantViolationException(
        $"Accounting subject {accountingSubjectId} must be recognized before authority can be assigned.");
