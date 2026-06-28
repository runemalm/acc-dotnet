using ACC.Authority.Application.Ports.Identity;
using ACC.ChartOfAccounts.Application.Ports.Templates;

namespace ACC.Application.Tests.TestKit;

internal sealed class TestRecognizedUserPort : IRecognizedUserPort
{
    private readonly HashSet<Guid> userIds = [];

    public bool IsRecognizedUser(Guid userId) =>
        userIds.Contains(userId);

    public void Recognize(Guid userId) =>
        userIds.Add(userId);
}

internal sealed class TestChartOfAccountsTemplateCatalog : IChartOfAccountsTemplateCatalog
{
    private readonly Dictionary<string, ChartOfAccountsTemplate> templates =
        new(StringComparer.OrdinalIgnoreCase);

    public ChartOfAccountsTemplate? Find(string templateId) =>
        templates.GetValueOrDefault(templateId);

    public IReadOnlyCollection<ChartOfAccountsTemplate> List() =>
        templates.Values.ToArray();

    public void Add(ChartOfAccountsTemplate template) =>
        templates[template.Id] = template;
}
