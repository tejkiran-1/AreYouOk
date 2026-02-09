using System.Globalization;
using Microsoft.Maui.Graphics;

namespace AreYouOk.MobileApp.Converters;

public class JourneyStatusToColorConverter : IValueConverter
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
                _ => Colors.LightGray
            };
        }
        return Colors.LightGray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
