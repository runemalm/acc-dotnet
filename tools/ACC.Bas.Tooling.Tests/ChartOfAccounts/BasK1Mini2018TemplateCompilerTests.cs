using System.Text;
using ACC.Bas.Tooling.ChartOfAccounts.Generation;
using ACC.Bas.Tooling.ChartOfAccounts.Generation.Compilers;
using ACC.Bas.Tooling.Publications.Parsing;
using ACC.Bas.Tooling.Workbooks;
using Xunit;

namespace ACC.Bas.Tooling.Tests.ChartOfAccounts;

public sealed class BasK1Mini2018TemplateCompilerTests
{
    private readonly IReadOnlyCollection<TemplateAccount> accounts;

    public BasK1Mini2018TemplateCompilerTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var compiler = new BasK1Mini2018TemplateCompiler(
            new ExcelWorkbookReader(),
            new BasK1WorkbookParser(),
            new BasK1MiniWorkbookParser());
        var sourceDirectory = Path.Combine(AppContext.BaseDirectory, "Sources");

        accounts = compiler.Compile(
            new TemplateRecipe(
                "test-template",
                "Test template",
                compiler.Name,
                new Dictionary<string, string>(),
                "unused.json"),
            new Dictionary<string, string>
            {
                ["accountDefinitions"] = Path.Combine(
                    sourceDirectory,
                    "Kontoplan_K1_2018.xls"),
                ["accountSelection"] = Path.Combine(
                    sourceDirectory,
                    "Kontoplan_K1_Mini_2018.xls")
            });
    }

    [Fact]
    public void ExactGroupAccount_IsIncluded()
    {
        var account = Assert.Single(accounts, account => account.Number == "1000");

        Assert.Equal("Immateriella anläggningstillgångar", account.Name);
    }

    [Fact]
    public void ExactMainAccounts_AreIncluded()
    {
        Assert.Contains(accounts, account => account.Number == "1110");
        Assert.Contains(accounts, account => account.Number == "1150");
    }

    [Fact]
    public void MainAccountRange_IsInclusive()
    {
        var selectedNumbers = accounts
            .Where(account => int.Parse(account.Number) is >= 2610 and <= 2650)
            .Select(account => account.Number)
            .ToArray();

        Assert.Equal(
            ["2610", "2620", "2630", "2640", "2650"],
            selectedNumbers);
    }

    [Fact]
    public void SubAccounts_AreExcluded()
    {
        Assert.All(accounts, account => Assert.EndsWith("0", account.Number));
        Assert.DoesNotContain(accounts, account => account.Number == "2611");
    }
}
