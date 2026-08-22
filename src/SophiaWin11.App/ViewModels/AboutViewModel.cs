using System.IO;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Services;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SophiaWin11.App.ViewModels;

public sealed partial class AboutViewModel : ObservableObject
{
    private readonly ILegalDocumentService _legalDocumentService;
    private readonly ILocalizationService _localizationService;
    private readonly ISnackbarService _snackbarService;

    public AboutViewModel(ILegalDocumentService legalDocumentService, ILocalizationService localizationService, ISnackbarService snackbarService)
    {
        _legalDocumentService = legalDocumentService;
        _localizationService = localizationService;
        _snackbarService = snackbarService;

        var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
        VersionDisplay = AppVersionFormatter.ToDisplayString(version);
    }

    public string AppName => "Sophia Script for Win11";

    public string VersionDisplay { get; }

    public string SophiaScriptCopyright => "© 2026 Dmitry Nefedov";

    public string SophiaScriptUrl => "https://github.com/farag2/Sophia-Script-for-Windows";

    public string UiWindowsCopyright => "© 2026 Patrick JAILLET";

    public string UiWindowsEmail => "sandefjord.development@proton.me";

    public string UiWindowsUrl => "https://patrickjaillet.github.io/sophia-win11";

    [RelayCommand]
    private void ExportLegalDocuments()
    {
        var dialog = new SaveFileDialog
        {
            Filter = $"{_localizationService.GetString("About_ExportFileTypeName")} (*.txt)|*.txt",
            FileName = "SophiaWin11-license-and-notices.txt",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            File.WriteAllText(dialog.FileName, _legalDocumentService.BuildExportDocument());

            _snackbarService.Show(
                _localizationService.GetString("About_SnackbarExportedTitle"),
                string.Format(_localizationService.GetString("About_SnackbarExportedMessage"), Path.GetFileName(dialog.FileName)),
                ControlAppearance.Success,
                TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            _snackbarService.Show(
                _localizationService.GetString("About_SnackbarExportFailedTitle"),
                ex.Message,
                ControlAppearance.Danger,
                TimeSpan.FromSeconds(5));
        }
    }
}
