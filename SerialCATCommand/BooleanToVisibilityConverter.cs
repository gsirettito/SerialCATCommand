using System.Windows;
using System.Windows.Data;

namespace SerialCATCommand {
    public class BooleanToVisibilityConverter: IValueConverter {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is bool && (bool)value)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is Visibility && (Visibility)value == Visibility.Visible)
                return true;
            return false;
        }
    }
}
