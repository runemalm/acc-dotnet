# ACC.Bas.Tooling

ACC.Bas.Tooling interprets publications from BAS-intressenternas Förening and derives artifacts used by ACC. Its first capability generates deterministic chart-of-accounts templates consumed by `ACC.ChartOfAccounts`.

```text
BAS publications
    -> publication parsers
    -> BAS template compiler
    -> ACC chart template resource
```

The publication parsers preserve BAS-specific knowledge independently from the artifacts derived from it. Future capabilities may use the same publications to derive reporting mappings or compare publication versions.

Available compilers:

| Compiler | Purpose |
| --- | --- |
| `bas-k1-2018` | Generates the full BAS K1 2018 template from its account definitions. |
| `bas-k1-mini-2018` | Resolves exact group/main accounts and inclusive main-account ranges against the BAS K1 2018 definitions. |

Generate a template from the repository root:

```bash
dotnet run --project tools/ACC.Bas.Tooling -- \
  generate tools/ACC.Bas.Tooling/ChartOfAccounts/Recipes/bas-k1-2018.recipe.json

dotnet run --project tools/ACC.Bas.Tooling -- \
  generate tools/ACC.Bas.Tooling/ChartOfAccounts/Recipes/bas-k1-mini-2018.recipe.json
```

Generated templates contain a stable identity, name, source artifact hashes, and the resolved accounts required by `ACC.ChartOfAccounts`. Workbook reporting mappings are intentionally outside chart templates.

The normal BAS 2018 and BAS 2026 workbooks are retained as source material for continued discovery. They do not currently produce chart templates.

Run the source-backed tooling tests from the repository root:

```bash
dotnet test tools/ACC.Bas.Tooling.Tests/ACC.Bas.Tooling.Tests.csproj
```
