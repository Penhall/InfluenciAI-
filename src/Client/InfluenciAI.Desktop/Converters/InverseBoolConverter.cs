using System;
using System.Globalization;
using System.Windows.Data;

namespace InfluenciAI.Desktop.Converters;

/// <summary>
/// Converts a boolean value to its inverse (true → false, false → true)
/// Used to disable controls when Busy = true
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? !b : true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? !b : true;
    }
}
