using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ProcessSH.Utils;

public sealed class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string s)
            return string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible;
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}