using ACC.ChartOfAccounts.Application.Ports.Templates;

namespace ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccountsTemplates;

public sealed class ViewChartOfAccountsTemplatesHandler
{
    private readonly IChartOfAccountsTemplateCatalog templates;

    public ViewChartOfAccountsTemplatesHandler(IChartOfAccountsTemplateCatalog templates)
    {
        this.templates = templates;
    }

    public ViewChartOfAccountsTemplatesResponse Handle(
        ViewChartOfAccountsTemplatesQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return new ViewChartOfAccountsTemplatesResponse(
            templates.List()
                .Select(template => new ChartOfAccountsTemplateResponse(
                    template.Id,
                    template.Name,
                    template.Accounts.Count))
                .ToArray());
    }
}
