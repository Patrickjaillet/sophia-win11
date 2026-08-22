using SophiaWin11.Core.Services;
using Xunit;

namespace SophiaWin11.Tests.Services;

public sealed class LegalDocumentServiceTests
{
    [Fact]
    public void GetLicenseText_ContainsMitGrantAndBothCopyrightHolders()
    {
        var service = new LegalDocumentService();

        var license = service.GetLicenseText();

        Assert.Contains("MIT License", license);
        Assert.Contains("Dmitry Nefedov", license);
        Assert.Contains("Patrick JAILLET", license);
    }

    [Fact]
    public void GetThirdPartyNoticesText_ContainsCoreDependencies()
    {
        var service = new LegalDocumentService();

        var notices = service.GetThirdPartyNoticesText();

        Assert.Contains("WPF-UI", notices);
        Assert.Contains("CommunityToolkit.Mvvm", notices);
        Assert.Contains("Microsoft.PowerShell.SDK", notices);
    }

    [Fact]
    public void BuildExportDocument_ConcatenatesBothDocumentsWithClearSeparators()
    {
        var service = new LegalDocumentService();

        var export = service.BuildExportDocument();

        Assert.Contains("LICENSE", export);
        Assert.Contains("THIRD-PARTY-NOTICES", export);
        Assert.Contains(service.GetLicenseText(), export);
        Assert.Contains(service.GetThirdPartyNoticesText(), export);
        Assert.True(export.IndexOf("LICENSE", StringComparison.Ordinal) < export.IndexOf("THIRD-PARTY-NOTICES", StringComparison.Ordinal));
    }
}
