using ShodanClient.App.Controls;

namespace ShodanClient.App.ViewModels.Trends;

/// <summary>
///     One value's month-by-month series within a requested Trends facet (e.g. the <c>US</c> line of
///     the <c>country</c> facet), ready to be plotted by a <see cref="TrendChart" />.
/// </summary>
/// <param name="FacetName">The facet this series belongs to, e.g. <c>country</c>.</param>
/// <param name="Value">The facet bucket's value, e.g. <c>US</c>.</param>
/// <param name="Points">The value's month-by-month counts, in chronological order.</param>
public sealed record TrendFacetValueSeries(string FacetName, string Value, IReadOnlyList<TrendPoint> Points)
{
	/// <summary>A display header combining the facet name and value, e.g. <c>country: US</c>.</summary>
	public string Header => $"{FacetName}: {Value}";
}
