using ACC.BuildingBlocks.Failures;

namespace ACC.Authority.Domain.Invariants;

public static class AccountingSubjectMustBeRecognizedForAuthority
{
    public static void Ensure(bool isRecognized, Guid accountingSubjectId)
    {
        if (!isRecognized)
        {
            throw new ResourceNotFoundException(
                $"Accounting subject {accountingSubjectId} must be recognized before authority can be assigned.");
        }
    }
}
