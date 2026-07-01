using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ShodanClient.App.Converters;

/// <summary>
///     Maps a <see langword="bool" /> to friendly display text instead of the raw <c>True</c>/<c>False</c>.
///     The converter parameter is <c>"TrueText|FalseText"</c>, e.g. <c>"Yes|No"</c>.
/// </summary>
public sealed class BoolToTextConverter : IValueConverter
{
	/// <inheritdoc />
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not bool flag)
		{
			return null;
		}

		var options = (parameter as string)?.Split('|', 2);
		if (options is not { Length: 2 })
		{
			return flag ? "Yes" : "No";
		}

		return flag ? options[0] : options[1];
	}

	/// <inheritdoc />
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		AvaloniaProperty.UnsetValue;
}
