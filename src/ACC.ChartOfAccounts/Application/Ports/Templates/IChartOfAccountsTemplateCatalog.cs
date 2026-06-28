namespace ACC.ChartOfAccounts.Application.Ports.Templates;

public interface IChartOfAccountsTemplateCatalog
{
    ChartOfAccountsTemplate? Find(string templateId);

    IReadOnlyCollection<ChartOfAccountsTemplate> List();
}
