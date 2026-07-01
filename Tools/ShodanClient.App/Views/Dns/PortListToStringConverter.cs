using System.Globalization;
using Avalonia.Data.Converters;

namespace ShodanClient.App.Views.Dns;

/// <summary>Joins a DNS record's <c>Ports</c> list into a comma-separated string for display.</summary>
public sealed class PortListToStringConverter : IValueConverter
{
	/// <summary>A shared, stateless instance for use from XAML via <c>{x:Static}</c>.</summary>
	public static readonly PortListToStringConverter Instance = new();

	/// <inheritdoc />
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value is IEnumerable<int> ports ? string.Join(", ", ports) : string.Empty;

	/// <inheritdoc />
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
