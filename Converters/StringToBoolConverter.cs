using System.Globalization;

namespace LocatCam_mobile.Converters;

/// <summary>
/// Convertit une valeur en booléen :
/// - string  : true si non null et non vide ;
/// - bool    : tel quel ;
/// - nombre  : true si strictement supérieur à 0 (ex. badge de messages non lus) ;
/// - autre   : true si non null.
/// Utilisé pour IsVisible / IsEnabled dans les écrans XAML.
/// </summary>
public class StringToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            null => false,
            bool b => b,
            string s => !string.IsNullOrWhiteSpace(s),
            int i => i > 0,
            long l => l > 0,
            _ => true
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
