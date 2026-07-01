using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ShodanClient.App.Converters;

/// <summary>Maps a banner's <c>transport</c> string (<c>tcp</c>/<c>udp</c>) to a short glyph.</summary>
public sealed class PortTransportToGlyphConverter : IValueConverter
{
	/// <inheritdoc />
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		(value as string)?.ToUpperInvariant() switch
		{
			"TCP" => "▲",
			"UDP" => "●",
			_ => "○"
		};

	/// <inheritdoc />
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		AvaloniaProperty.UnsetValue;
}
