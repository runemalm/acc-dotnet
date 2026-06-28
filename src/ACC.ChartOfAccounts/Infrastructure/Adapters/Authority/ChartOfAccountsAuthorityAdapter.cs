using ACC.Authority.Application.Policies;
using ACC.Authority.Domain.Powers;
using ACC.ChartOfAccounts.Application.Ports.Authority;

namespace ACC.ChartOfAccounts.Infrastructure.Adapters.Authority;

public sealed class ChartOfAccountsAuthorityAdapter : IChartOfAccountsAuthorityPort
{
    private readonly IAuthorityPolicy authorityPolicy;

    public ChartOfAccountsAuthorityAdapter(IAuthorityPolicy authorityPolicy)
    {
        this.authorityPolicy = authorityPolicy;
    }

    public bool CanAdoptChartOfAccounts(Guid actorUserId, Guid accountingSubjectId) =>
        authorityPolicy.HasPower(actorUserId, accountingSubjectId, Power.AdoptChartOfAccounts);

    public bool CanManageChartOfAccounts(Guid actorUserId, Guid accountingSubjectId) =>
        authorityPolicy.HasPower(actorUserId, accountingSubjectId, Power.ManageChartOfAccounts);
}
