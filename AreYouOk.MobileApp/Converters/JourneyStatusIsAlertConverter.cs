using System.Globalization;

namespace AreYouOk.MobileApp.Converters;

public class JourneyStatusIsAlertConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            var lowerStatus = status?.ToLower();
            return lowerStatus == "alert" || lowerStatus == "sos";
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
