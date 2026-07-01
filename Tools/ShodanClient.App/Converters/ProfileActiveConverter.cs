using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ShodanClient.App.Converters;

/// <summary>
///     Compares two <see cref="Guid" />-shaped bindings for equality — typically a profile row's
///     <c>Id</c> (first binding) against some ambient "current" id such as the active profile or the
///     profile currently being renamed (second binding) — to highlight/select a single row in a list.
///     The converter parameter selects the output: <c>"Check"</c> for a <see langword="bool" /> (e.g. a
///     checkmark's <c>IsVisible</c>), <c>"NotCheck"</c> for its negation (e.g. "hide this row's normal
///     content while it's the one being renamed"), or omitted for the default highlight brush.
/// </summary>
public sealed class ProfileActiveConverter : IMultiValueConverter
{
	private static readonly IBrush ActiveBackground = new SolidColorBrush(Color.Parse("#22FA3D3D"));

	/// <inheritdoc />
	public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
	{
		var isMatch = values is [Guid rowId, Guid otherId] && rowId == otherId;

		return (parameter as string)?.ToUpperInvariant() switch
		{
			"CHECK" => isMatch,
			"NOTCHECK" => !isMatch,
			_ => isMatch ? ActiveBackground : Brushes.Transparent
		};
	}
}
