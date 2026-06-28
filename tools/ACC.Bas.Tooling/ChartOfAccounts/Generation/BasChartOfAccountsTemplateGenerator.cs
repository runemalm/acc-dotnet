using System.Security.Cryptography;
using System.Text.Json;

namespace ACC.Bas.Tooling.ChartOfAccounts.Generation;

internal sealed class BasChartOfAccountsTemplateGenerator
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IReadOnlyDictionary<string, IChartOfAccountsTemplateCompiler> compilers;
    private readonly TemplateResourceWriter writer;

    public BasChartOfAccountsTemplateGenerator(
        IEnumerable<IChartOfAccountsTemplateCompiler> compilers,
        TemplateResourceWriter writer)
    {
        this.compilers = compilers.ToDictionary(compiler => compiler.Name, StringComparer.OrdinalIgnoreCase);
        this.writer = writer;
    }

    public async Task<TemplateGenerationResult> Generate(string recipePath)
    {
        var recipe = JsonSerializer.Deserialize<TemplateRecipe>(
                         await File.ReadAllTextAsync(recipePath),
                         SerializerOptions)
                     ?? throw new InvalidOperationException($"Template recipe {recipePath} is empty.");
        Validate(recipe);

        if (!compilers.TryGetValue(recipe.Compiler, out var compiler))
        {
            throw new InvalidOperationException($"Template compiler '{recipe.Compiler}' is not recognized.");
        }

        var directory = Path.GetDirectoryName(recipePath)!;
        var inputs = recipe.Inputs.ToDictionary(
            input => input.Key,
            input => Path.GetFullPath(Path.Combine(directory, input.Value)),
            StringComparer.OrdinalIgnoreCase);
        var accounts = compiler.Compile(recipe, inputs)
            .OrderBy(account => account.Number, StringComparer.Ordinal)
            .ToArray();
        EnsureUniqueAccounts(accounts);

        var sourceArtifacts = new List<SourceArtifact>();
        foreach (var input in inputs.OrderBy(input => input.Key, StringComparer.Ordinal))
        {
            sourceArtifacts.Add(new SourceArtifact(
                Path.GetFileName(input.Value),
                await Hash(input.Value)));
        }

        var resource = new ChartOfAccountsTemplateResource(
            recipe.Id,
            recipe.Name,
            sourceArtifacts,
            accounts);
        var outputPath = Path.GetFullPath(Path.Combine(directory, recipe.Output));
        await writer.Write(outputPath, resource);

        return new TemplateGenerationResult(outputPath, accounts.Length);
    }

    private static void Validate(TemplateRecipe recipe)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipe.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipe.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipe.Compiler);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipe.Output);

        if (recipe.Inputs.Count == 0)
        {
            throw new InvalidOperationException("A template recipe must identify its source inputs.");
        }
    }

    private static void EnsureUniqueAccounts(IReadOnlyCollection<TemplateAccount> accounts)
    {
        if (accounts.Count == 0)
        {
            throw new InvalidOperationException("A generated template must contain accounts.");
        }

        var duplicate = accounts.GroupBy(account => account.Number, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1)?.Key;
        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Generated account number {duplicate} is not unique.");
        }
    }

    private static async Task<string> Hash(string path) =>
        Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(path)))
            .ToLowerInvariant();
}

internal sealed record TemplateGenerationResult(
    string OutputPath,
    int AccountCount);
