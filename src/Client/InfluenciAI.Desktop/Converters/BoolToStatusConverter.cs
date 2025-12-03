using System;
using System.Globalization;
using System.Windows.Data;

namespace InfluenciAI.Desktop.Converters;

/// <summary>
/// Converts boolean to status text (true = "Ativo", false = "Inativo")
/// Used for status badges
/// </summary>
public class BoolToStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? "Ativo" : "Inativo";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
