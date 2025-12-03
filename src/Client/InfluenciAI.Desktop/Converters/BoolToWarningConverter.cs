using System;
using System.Globalization;
using System.Windows.Data;

namespace InfluenciAI.Desktop.Converters;

/// <summary>
/// Converts boolean to warning message (true = warning, false = empty)
/// Used to show character limit warnings
/// </summary>
public class BoolToWarningConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? "⚠ Limite de caracteres excedido!" : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
