using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SwpfEditor.Domain.Services;

namespace SwpfEditor.App.Converters;

public class SeverityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ValidationSeverity severity)
        {
            return severity switch
            {
                ValidationSeverity.Error => Brushes.Red,
                ValidationSeverity.Warning => Brushes.Orange,
                ValidationSeverity.Info => Brushes.Blue,
                _ => Brushes.Black
            };
        }
        return Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}