namespace ShodanClient.Domain.Search;

/// <summary>
///     The result of <c>GET /shodan/host/count</c>: the total number of matches and any requested
///     facet breakdowns, without consuming query credits and without returning banners.
/// </summary>
public sealed record CountResult
{
	/// <summary>Total number of matches for the query (<c>total</c>).</summary>
	public long Total { get; init; }

	/// <summary>Facet breakdowns keyed by facet name (<c>facets</c>).</summary>
	public IReadOnlyDictionary<string, IReadOnlyList<FacetItem>> Facets { get; init; } =
		new Dictionary<string, IReadOnlyList<FacetItem>>();
}
