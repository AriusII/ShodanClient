namespace ShodanClient.Domain.Trends;

/// <summary>
///     The result of a Trends search (<c>GET /api/v1/search</c> on <c>trends.shodan.io</c>): a
///     month-by-month historical breakdown of how many results matched a query, plus optional
///     month-by-month facet summaries.
/// </summary>
public sealed record TrendResult
{
	/// <summary>Total number of matches across every month returned (<c>total</c>).</summary>
	public long Total { get; init; }

	/// <summary>Month-by-month match counts (<c>matches</c>).</summary>
	public IReadOnlyList<TrendMatch> Matches { get; init; } = [];

	/// <summary>Month-by-month facet breakdowns keyed by facet name (<c>facets</c>).</summary>
	public IReadOnlyDictionary<string, IReadOnlyList<TrendFacetGroup>> Facets { get; init; } =
		new Dictionary<string, IReadOnlyList<TrendFacetGroup>>();
}
