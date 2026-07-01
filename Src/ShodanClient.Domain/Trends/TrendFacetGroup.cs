using ShodanClient.Domain.Search;

namespace ShodanClient.Domain.Trends;

/// <summary>
///     A single month's facet breakdown from a Trends search — one element of the array a
///     <see cref="TrendResult" /> keeps per facet name in its <c>facets</c> map.
/// </summary>
public sealed record TrendFacetGroup
{
	/// <summary>The month the breakdown applies to, formatted <c>YYYY-MM</c> (<c>key</c>).</summary>
	public required string Key { get; init; }

	/// <summary>The facet buckets for that month (<c>values</c>).</summary>
	public IReadOnlyList<FacetItem> Values { get; init; } = [];
}
