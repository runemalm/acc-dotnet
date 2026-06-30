using ACC.BuildingBlocks.Domain;

namespace ACC.AccountingSubject.Domain.Invariants;

public static class AccountingSubjectOrganizationNumberMustBeUnique
{
    public static void Ensure(bool isUnique)
    {
        if (!isUnique)
        {
            throw new AccountingSubjectOrganizationNumberMustBeUniqueViolation();
        }
    }
}

public sealed class AccountingSubjectOrganizationNumberMustBeUniqueViolation()
    : InvariantViolationException(
        "An accounting subject with the same organization number already exists.");
