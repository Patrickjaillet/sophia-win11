using SophiaWin11.App.ViewModels;
using SophiaWin11.Core.Services;
using SophiaWin11.Tests.Fakes;
using Xunit;

namespace SophiaWin11.Tests.ViewModels;

public sealed class AboutViewModelTests
{
    [Fact]
    public void Construction_ExposesLiteralCopyrightAndLinkFields()
    {
        var viewModel = new AboutViewModel(new LegalDocumentService(), new FakeLocalizationService(), new FakeSnackbarService());

        Assert.Equal("Sophia Script for Win11", viewModel.AppName);
        Assert.Equal("© 2026 Dmitry Nefedov", viewModel.SophiaScriptCopyright);
        Assert.Equal("https://github.com/farag2/Sophia-Script-for-Windows", viewModel.SophiaScriptUrl);
        Assert.Equal("© 2026 Patrick JAILLET", viewModel.UiWindowsCopyright);
        Assert.Equal("sandefjord.development@proton.me", viewModel.UiWindowsEmail);
        Assert.Equal("https://patrickjaillet.github.io/sophia-win11", viewModel.UiWindowsUrl);
    }

    [Fact]
    public void Construction_FormatsVersionAsFourSegmentSemVer()
    {
        var viewModel = new AboutViewModel(new LegalDocumentService(), new FakeLocalizationService(), new FakeSnackbarService());

        Assert.Matches(@"^v\d+\.\d+\.\d+\.\d+$", viewModel.VersionDisplay);
    }
}
