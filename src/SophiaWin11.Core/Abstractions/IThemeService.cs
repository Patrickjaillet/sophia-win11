namespace SophiaWin11.Core.Abstractions;

public interface IThemeService
{
    bool IsThemeApplied { get; }

    void ApplyTheme();
}
