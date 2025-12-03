using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InfluenciAI.Desktop.Converters;

/// <summary>
/// Converts boolean to Visibility (true = Visible, false = Collapsed)
/// Used to show/hide UI elements
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility v && v == Visibility.Visible;
    }
}
