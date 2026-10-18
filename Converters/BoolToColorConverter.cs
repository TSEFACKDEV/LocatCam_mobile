using System.Globalization;
using LocatCam_mobile.Models;

namespace LocatCam_mobile.Converters;

/// <summary>
/// Convertit un booléen en couleur : true = succès (vert), false = erreur (rouge).
/// Le ConverterParameter peut être "Error;Success" pour spécifier les couleurs.
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isSuccess = value as bool? == true;
        var param = parameter as string ?? "Error;Success";
        var parts = param.Split(';');

        var colorName = isSuccess && parts.Length > 1 ? parts[1] : parts[0];

        return colorName switch
        {
            "Error" => Colors.Red,
            "Success" => Colors.Green,
            _ => Colors.Gray
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
