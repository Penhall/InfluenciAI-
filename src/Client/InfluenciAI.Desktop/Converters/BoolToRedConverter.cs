using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace InfluenciAI.Desktop.Converters;

/// <summary>
/// Converts boolean to color (true = red, false = black)
/// Used for character counter when over limit
/// </summary>
public class BoolToRedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isOverLimit && isOverLimit)
        {
            return new SolidColorBrush(Colors.Red);
        }
        return new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
