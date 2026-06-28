using ACC.ChartOfAccounts.Application.Ports.AccountingSubject;
using ACC.ChartOfAccounts.Application.Ports.Authority;
using ACC.ChartOfAccounts.Application.Ports.Templates;

namespace ACC.ChartOfAccounts.Tests.TestKit;

internal sealed class TestChartOfAccountsTemplateCatalog : IChartOfAccountsTemplateCatalog
{
    private readonly Dictionary<string, ChartOfAccountsTemplate> templates =
        new(StringComparer.OrdinalIgnoreCase);

    public ChartOfAccountsTemplate? Find(string templateId) =>
        templates.GetValueOrDefault(templateId);

    public IReadOnlyCollection<ChartOfAccountsTemplate> List() =>
        templates.Values.OrderBy(template => template.Name, StringComparer.Ordinal).ToArray();

    public void Add(ChartOfAccountsTemplate template) =>
        templates[template.Id] = template;
}

internal sealed class TestRecognizedAccountingSubjectPort : IRecognizedAccountingSubjectPort
{
    private readonly HashSet<Guid> accountingSubjectIds = [];

    public bool IsRecognizedAccountingSubject(Guid accountingSubjectId) =>
        accountingSubjectIds.Contains(accountingSubjectId);

    public void Recognize(Guid accountingSubjectId) =>
        accountingSubjectIds.Add(accountingSubjectId);
}

internal sealed class TestChartOfAccountsAuthorityPort : IChartOfAccountsAuthorityPort
{
    private readonly HashSet<(Guid UserId, Guid AccountingSubjectId)> adoptionPowers = [];
    private readonly HashSet<(Guid UserId, Guid AccountingSubjectId)> managementPowers = [];

    public bool CanAdoptChartOfAccounts(Guid actorUserId, Guid accountingSubjectId) =>
        adoptionPowers.Contains((actorUserId, accountingSubjectId));

    public bool CanManageChartOfAccounts(Guid actorUserId, Guid accountingSubjectId) =>
        managementPowers.Contains((actorUserId, accountingSubjectId));

    public void AllowAdoption(Guid actorUserId, Guid accountingSubjectId) =>
        adoptionPowers.Add((actorUserId, accountingSubjectId));

    public void AllowManagement(Guid actorUserId, Guid accountingSubjectId) =>
        managementPowers.Add((actorUserId, accountingSubjectId));
}
