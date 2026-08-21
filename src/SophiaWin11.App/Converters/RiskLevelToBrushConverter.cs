using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.App.Converters;

public sealed class RiskLevelToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TweakRiskLevel riskLevel || Application.Current is null)
        {
            return Brushes.Gray;
        }

        var resourceKey = riskLevel switch
        {
            TweakRiskLevel.Low => "BrushAccentEmerald",
            TweakRiskLevel.Medium => "BrushAccentGold",
            TweakRiskLevel.High => "BrushAccentBordeaux",
            _ => "BrushAccentGold",
        };

        return Application.Current.TryFindResource(resourceKey) as Brush ?? Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
