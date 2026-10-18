using System.Globalization;

namespace LocatCam_mobile.Converters;

/// <summary>
/// Formate un prix en FCFA (XAF) : "15 000 FCFA"
/// </summary>
public class PrixFcfaConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return "0 FCFA";

        decimal prix;
        try
        {
            prix = System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }
        catch
        {
            return "0 FCFA";
        }

        return $"{prix:N0} FCFA";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
