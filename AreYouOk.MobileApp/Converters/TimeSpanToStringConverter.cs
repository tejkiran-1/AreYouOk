using System.Globalization;

namespace AreYouOk.MobileApp.Converters;

public class TimeSpanToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                return $"{(int)timeSpan.TotalMinutes}m";
            }
            else
            {
                return $"{(int)timeSpan.TotalSeconds}s";
            }
        }
        return "0m";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
