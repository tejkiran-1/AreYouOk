using System.Globalization;

namespace AreYouOk.MobileApp.Converters;

public class JourneyStatusToEmojiConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status?.ToLower() switch
            {
                "safe" => "✅",
                "alert" => "⚠️",
                "sos" => "🆘",
                "active" => "🟢",
                "completed" => "✔️",
                _ => "⚪"
            };
        }
        return "⚪";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
