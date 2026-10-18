using System.Globalization;

namespace LocatCam_mobile.Converters;

/// <summary>
/// Convertit une string en booléen : true si la string n'est pas null ni vide.
/// </summary>
public class StringToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrWhiteSpace(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
