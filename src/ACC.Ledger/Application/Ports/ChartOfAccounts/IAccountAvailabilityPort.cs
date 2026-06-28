namespace ACC.Ledger.Application.Ports.ChartOfAccounts;

public interface IAccountAvailabilityPort
{
    PostingAccountAvailability GetAvailability(
        Guid accountingSubjectId,
        string accountNumber);
}

public enum PostingAccountAvailability
{
    Unrecognized,
    Inactive,
    Active
}
