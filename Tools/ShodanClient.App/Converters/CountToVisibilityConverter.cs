using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ShodanClient.App.Converters;

/// <summary>
///     Converts an <see cref="int" /> count to <see langword="true" /> when greater than zero. Unlike
///     <see cref="NullOrEmptyToVisibilityConverter" />, this is meant for binding directly to a live
///     <c>ObservableCollection{T}.Count</c> property (which itself raises property-changed
///     notifications on every add/remove) rather than to the collection instance, which never changes
///     reference and so would not refresh an empty-state overlay bound to it. Pass the converter
///     parameter <c>"Invert"</c> to flip the result.
/// </summary>
public sealed class CountToVisibilityConverter : IValueConverter
{
	/// <inheritdoc />
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var hasItems = value is > 0;
		var invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
		return invert ? !hasItems : hasItems;
	}

	/// <inheritdoc />
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		AvaloniaProperty.UnsetValue;
}
