using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ShodanClient.App.Converters;

/// <summary>
///     Formats a raw byte count (e.g. <see cref="ShodanClient.Domain.BulkData.DatasetFile.Size" />) as a
///     human-readable size such as <c>44.9 GB</c>, instead of a long, hard-to-scan digit string.
/// </summary>
public sealed class ByteSizeConverter : IValueConverter
{
	private static readonly string[] Units = ["B", "KB", "MB", "GB", "TB", "PB"];

	/// <inheritdoc />
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not long and not int)
		{
			return value;
		}

		var bytes = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
		if (bytes < 0)
		{
			return value;
		}

		var unitIndex = 0;
		while (bytes >= 1024 && unitIndex < Units.Length - 1)
		{
			bytes /= 1024;
			unitIndex++;
		}

		var format = unitIndex == 0 ? "{0:N0} {1}" : "{0:N1} {1}";
		return string.Format(culture, format, bytes, Units[unitIndex]);
	}

	/// <inheritdoc />
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		AvaloniaProperty.UnsetValue;
}
