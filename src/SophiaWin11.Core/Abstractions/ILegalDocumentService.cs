namespace SophiaWin11.Core.Abstractions;

public interface ILegalDocumentService
{
    string GetLicenseText();

    string GetThirdPartyNoticesText();

    string BuildExportDocument();
}
