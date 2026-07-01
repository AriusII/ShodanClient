namespace ShodanClient.Domain.Search;

/// <summary>
///     The parsed form of a search query from <c>GET /shodan/host/search/tokens</c>: which filters and
///     free-text the query resolves to, plus any parse errors.
/// </summary>
public sealed record QueryTokenParse
{
	/// <summary>The free-text portion of the query, with filters removed (<c>string</c>).</summary>
	public string? SearchTerm { get; init; }

	/// <summary>Filter values keyed by filter name (<c>filters</c>/<c>attributes</c>).</summary>
	public IReadOnlyDictionary<string, string> Attributes { get; init; } =
		new Dictionary<string, string>();

	/// <summary>The names of the filters used in the query (<c>filters</c>).</summary>
	public IReadOnlyList<string> Filters { get; init; } = [];

	/// <summary>Any errors encountered while parsing the query (<c>errors</c>).</summary>
	public IReadOnlyList<string> Errors { get; init; } = [];
}
