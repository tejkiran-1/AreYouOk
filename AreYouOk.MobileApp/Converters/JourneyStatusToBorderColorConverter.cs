using System.Globalization;
using Microsoft.Maui.Graphics;

namespace AreYouOk.MobileApp.Converters;

public class JourneyStatusToBorderColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status?.ToLower() switch
            {
                "safe" => Colors.Green,
                "alert" => Colors.Orange,
                "sos" => Colors.Red,
                "active" => Colors.Blue,
                "completed" => Colors.Gray,
                _ => Color.FromRgba("#E0E0E0")
            };
        }
        return Color.FromRgba("#E0E0E0");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
