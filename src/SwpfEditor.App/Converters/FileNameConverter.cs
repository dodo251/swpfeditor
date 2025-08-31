using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace SwpfEditor.App.Converters;

public class FileNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string filePath && !string.IsNullOrEmpty(filePath))
        {
            return Path.GetFileName(filePath);
        }
        return "无文件";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}