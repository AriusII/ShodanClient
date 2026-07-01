using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ShodanClient.App.Converters;

/// <summary>
///     Renders an SSL certificate's <c>Expired</c> flag as an at-a-glance colored badge instead of a
///     bare "True"/"False" string. Pass the converter parameter <c>"Text"</c> for the badge's label or
///     <c>"Brush"</c> (the default) for its background.
/// </summary>
public sealed class CertificateExpiredConverter : IValueConverter
{
	private static readonly IBrush ExpiredBrush = Brushes.Crimson;
	private static readonly IBrush ValidBrush = Brushes.SeaGreen;

	/// <inheritdoc />
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var expired = value is true;
		if (string.Equals(parameter as string, "Text", StringComparison.OrdinalIgnoreCase))
		{
			return expired ? "Expired" : "Valid";
		}

		return expired ? ExpiredBrush : ValidBrush;
	}

	/// <inheritdoc />
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		AvaloniaProperty.UnsetValue;
}
