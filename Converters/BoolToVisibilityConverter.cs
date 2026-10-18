using System.Globalization;
using Microsoft.Maui.Controls;

namespace LocatCam_mobile.Converters;

/// <summary>
/// Convertit un booléen en Visible/Collapsed.
/// ConverterParameter = "Inverse" pour inverser la logique.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isVisible = value as bool? == true;
        var inverse = parameter as string == "Inverse";
        if (inverse) isVisible = !isVisible;

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
