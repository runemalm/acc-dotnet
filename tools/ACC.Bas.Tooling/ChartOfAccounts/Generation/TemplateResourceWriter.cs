using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ACC.Bas.Tooling.ChartOfAccounts.Generation;

internal sealed class TemplateResourceWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    public async Task Write(string path, ChartOfAccountsTemplateResource resource)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(
            path,
            JsonSerializer.Serialize(resource, SerializerOptions) + Environment.NewLine,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }
}
