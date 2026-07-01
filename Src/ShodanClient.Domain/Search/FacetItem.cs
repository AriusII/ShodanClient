namespace ShodanClient.Domain.Search;

/// <summary>
///     A single facet bucket: how many results share a given value for a faceted property
///     (e.g. <c>value = "US", count = 1234</c> for the <c>country</c> facet).
/// </summary>
/// <param name="Value">The property value.</param>
/// <param name="Count">The number of results with that value.</param>
public readonly record struct FacetItem(string Value, long Count);
