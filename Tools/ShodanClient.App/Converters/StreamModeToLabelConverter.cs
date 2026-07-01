using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using ShodanClient.App.ViewModels.Streaming;

namespace ShodanClient.App.Converters;

/// <summary>
///     Maps a <see cref="StreamMode" /> to a human-friendly label for the Streaming module's mode
///     picker, since the raw enum names for <see cref="StreamMode.Alert" /> and
///     <see cref="StreamMode.Alerts" /> differ by a single character but mean very different things
///     ("this one alert" versus "every alert on the account").
/// </summary>
public sealed class StreamModeToLabelConverter : IValueConverter
{
	/// <inheritdoc />
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value switch
		{
			StreamMode.AllBanners => "All banners",
			StreamMode.Asn => "By ASN",
			StreamMode.Countries => "By country",
			StreamMode.Ports => "By port",
			StreamMode.Vulnerabilities => "By vulnerability (CVE)",
			StreamMode.Query => "By query",
			StreamMode.Alerts => "All alerts",
			StreamMode.Alert => "Single alert (by ID)",
			_ => value?.ToString() ?? string.Empty
		};

	/// <inheritdoc />
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		AvaloniaProperty.UnsetValue;
}
