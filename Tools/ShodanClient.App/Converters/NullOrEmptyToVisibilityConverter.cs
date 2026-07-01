using System.Collections;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ShodanClient.App.Converters;

/// <summary>
///     Converts <see langword="null" />, an empty string, or an empty collection to
///     <see langword="false" /> (hide), and anything else to <see langword="true" /> (show). Pass
///     the converter parameter <c>"Invert"</c> to flip the result.
/// </summary>
public sealed class NullOrEmptyToVisibilityConverter : IValueConverter
{
	/// <inheritdoc />
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var hasContent = value switch
		{
			null => false,
			string text => !string.IsNullOrWhiteSpace(text),
			ICollection collection => collection.Count > 0,
			IEnumerable enumerable => enumerable.GetEnumerator().MoveNext(),
			_ => true
		};

		var invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
		return invert ? !hasContent : hasContent;
	}

	/// <inheritdoc />
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		AvaloniaProperty.UnsetValue;
}
