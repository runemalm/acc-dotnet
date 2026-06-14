namespace ACC.Authority.Domain.Invariants;

public static class AccountingSubjectMustBeRecognizedForAuthority
{
    public static void Ensure(bool isRecognized, Guid accountingSubjectId)
    {
        if (!isRecognized)
        {
            throw new InvalidOperationException(
                $"Accounting subject {accountingSubjectId} must be recognized before authority can be assigned.");
        }
    }
}
