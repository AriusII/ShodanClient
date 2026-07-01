namespace ShodanClient.Domain.Directory;

/// <summary>
///     A page of saved queries returned by <c>GET /shodan/query</c> or <c>GET /shodan/query/search</c>.
/// </summary>
public sealed record SavedQueryResult
{
	/// <summary>The saved queries on this page (<c>matches</c>).</summary>
	public IReadOnlyList<SavedQuery> Matches { get; init; } = [];

	/// <summary>Total number of saved queries matching the request (<c>total</c>).</summary>
	public long Total { get; init; }
}
