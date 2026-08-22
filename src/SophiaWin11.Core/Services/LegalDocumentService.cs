using System.Reflection;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class LegalDocumentService : ILegalDocumentService
{
    private const string LicenseResourceName = "SophiaWin11.Core.Legal.LICENSE.txt";
    private const string ThirdPartyNoticesResourceName = "SophiaWin11.Core.Legal.THIRD-PARTY-NOTICES.md";

    private readonly Lazy<string> _licenseText;
    private readonly Lazy<string> _thirdPartyNoticesText;

    public LegalDocumentService()
    {
        _licenseText = new Lazy<string>(() => ReadEmbeddedResource(LicenseResourceName));
        _thirdPartyNoticesText = new Lazy<string>(() => ReadEmbeddedResource(ThirdPartyNoticesResourceName));
    }

    public string GetLicenseText() => _licenseText.Value;

    public string GetThirdPartyNoticesText() => _thirdPartyNoticesText.Value;

    public string BuildExportDocument()
    {
        var separator = new string('=', 72);

        return string.Join(
            Environment.NewLine + Environment.NewLine,
            $"{separator}{Environment.NewLine}LICENSE{Environment.NewLine}{separator}",
            GetLicenseText(),
            $"{separator}{Environment.NewLine}THIRD-PARTY-NOTICES{Environment.NewLine}{separator}",
            GetThirdPartyNoticesText());
    }

    private static string ReadEmbeddedResource(string resourceName)
    {
        var assembly = typeof(LegalDocumentService).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded legal resource '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
