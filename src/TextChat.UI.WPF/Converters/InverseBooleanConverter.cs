using System.Globalization;
using System.Windows.Data;

namespace TextChat.UI.WPF.Converters;

internal class InverseBooleanConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool booleanValue)
			return !booleanValue;

		throw new ArgumentException("Value must be a boolean", nameof(value));
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool booleanValue)
			return !booleanValue;

		throw new ArgumentException("Value must be a boolean", nameof(value));
	}
}