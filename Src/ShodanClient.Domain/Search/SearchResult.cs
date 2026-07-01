namespace ShodanClient.Domain.Search;

/// <summary>
///     A page of search results from <c>GET /shodan/host/search</c>: the matching banners, the total
///     number of matches across all pages, and any requested facet breakdowns.
/// </summary>
public sealed record SearchResult
{
	/// <summary>The banners matching the query for this page (<c>matches</c>).</summary>
	public IReadOnlyList<Banner> Matches { get; init; } = [];

	/// <summary>Total number of matches across all pages (<c>total</c>).</summary>
	public long Total { get; init; }

	/// <summary>Facet breakdowns keyed by facet name, present only if facets were requested (<c>facets</c>).</summary>
	public IReadOnlyDictionary<string, IReadOnlyList<FacetItem>> Facets { get; init; } =
		new Dictionary<string, IReadOnlyList<FacetItem>>();
}
