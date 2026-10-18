using System.Globalization;

namespace LocatCam_mobile.Converters;

/// <summary>
/// Convertit une DateTime en chaîne relative (ex: "il y a 2 heures", "hier", "15 oct.").
/// </summary>
public class DateTimeToRelativeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        DateTime date;
        try
        {
            date = System.Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }
        catch
        {
            return string.Empty;
        }

        var now = DateTime.UtcNow;
        var diff = now - date;

        if (diff.TotalSeconds < 60)
            return "À l'instant";
        if (diff.TotalMinutes < 60)
            return $"Il y a {(int)diff.TotalMinutes} min";
        if (diff.TotalHours < 24)
            return $"Il y a {(int)diff.TotalHours} h";
        if (diff.TotalDays < 2)
            return "Hier";
        if (diff.TotalDays < 7)
            return $"Il y a {(int)diff.TotalDays} j";

        return date.ToString("dd MMM", culture);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
