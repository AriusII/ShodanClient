using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ShodanClient.Domain.Search;

namespace ShodanClient.App.Converters;

/// <summary>Maps a <see cref="Vulnerability" /> to a severity-colored brush (KEV/CVSS-based).</summary>
/// <remarks>
///     Every tone below keeps white badge text at/above the WCAG AA 4.5:1 contrast minimum for
///     12px text (the stock <c>Goldenrod</c>/<c>SlateGray</c>/<c>Gray</c> brushes this used to use
///     did not). <see cref="Kev" /> is deliberately the most saturated, alarm-red tone of the set —
///     brighter than <see cref="Critical" /> — so CISA-known-exploited CVEs visually outrank plain
///     "Critical" CVSS scores, matching <see cref="Vulnerability.Kev" />'s real-world severity.
/// </remarks>
public sealed class CveSeverityToBrushConverter : IValueConverter
{
	private static readonly IBrush Kev = new SolidColorBrush(Color.Parse("#E8001F"));
	private static readonly IBrush Critical = Brushes.Crimson;
	private static readonly IBrush High = new SolidColorBrush(Color.Parse("#C1440E"));
	private static readonly IBrush Medium = new SolidColorBrush(Color.Parse("#8A5A00"));
	private static readonly IBrush Low = new SolidColorBrush(Color.Parse("#3F4B57"));
	private static readonly IBrush Unknown = new SolidColorBrush(Color.Parse("#595959"));

	/// <inheritdoc />
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value switch
		{
			Vulnerability { Kev: true } => Kev,
			Vulnerability { Cvss: >= 9.0 } => Critical,
			Vulnerability { Cvss: >= 7.0 } => High,
			Vulnerability { Cvss: >= 4.0 } => Medium,
			Vulnerability { Cvss: not null } => Low,
			_ => Unknown
		};

	/// <inheritdoc />
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		AvaloniaProperty.UnsetValue;
}
