using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace InfluenciAI.Desktop.Converters;

/// <summary>
/// Converts boolean to color (true = green, false = red)
/// Used for status badges
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return new SolidColorBrush(isActive ? Colors.Green : Colors.Red);
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
