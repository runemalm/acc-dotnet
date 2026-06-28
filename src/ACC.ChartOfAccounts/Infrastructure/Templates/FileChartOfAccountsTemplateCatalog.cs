using System.Text.Json;
using ACC.ChartOfAccounts.Application.Ports.Templates;

namespace ACC.ChartOfAccounts.Infrastructure.Templates;

public sealed class FileChartOfAccountsTemplateCatalog : IChartOfAccountsTemplateCatalog
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly string resourceDirectory;

    public FileChartOfAccountsTemplateCatalog()
        : this(Path.Combine(AppContext.BaseDirectory, "ChartOfAccounts", "Templates"))
    {
    }

    public FileChartOfAccountsTemplateCatalog(string resourceDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceDirectory);
        this.resourceDirectory = resourceDirectory;
    }

    public ChartOfAccountsTemplate? Find(string templateId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);

        return List().SingleOrDefault(template =>
            string.Equals(template.Id, templateId, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyCollection<ChartOfAccountsTemplate> List()
    {
        if (!Directory.Exists(resourceDirectory))
        {
            return Array.Empty<ChartOfAccountsTemplate>();
        }

        var templates = Directory.EnumerateFiles(
                resourceDirectory,
                "*.template.json",
                SearchOption.AllDirectories)
            .Select(ReadTemplate)
            .OrderBy(template => template.Name, StringComparer.Ordinal)
            .ThenBy(template => template.Id, StringComparer.Ordinal)
            .ToArray();

        var duplicateId = templates
            .GroupBy(template => template.Id, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1)?.Key;
        if (duplicateId is not null)
        {
            throw new InvalidOperationException(
                $"More than one chart of accounts template has the identity {duplicateId}.");
        }

        return templates;
    }

    private static ChartOfAccountsTemplate ReadTemplate(string file)
    {
        using var stream = File.OpenRead(file);
        var resource = JsonSerializer.Deserialize<TemplateResource>(stream, SerializerOptions)
            ?? throw new InvalidOperationException($"Chart of accounts template {file} is empty.");

        return new ChartOfAccountsTemplate(
            resource.Id,
            resource.Name,
            resource.Accounts
                .Select(account => new TemplateAccount(account.Number, account.Name))
                .ToArray());
    }

    private sealed record TemplateResource(
        string Id,
        string Name,
        IReadOnlyCollection<AccountResource> Accounts);

    private sealed record AccountResource(
        string Number,
        string Name);
}
