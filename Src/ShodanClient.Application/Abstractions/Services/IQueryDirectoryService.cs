using ShodanClient.Domain.Directory;

namespace ShodanClient.Application.Abstractions.Services;

/// <summary>
///     Browsing and searching the public directory of saved Shodan search queries. Exposed on the
///     client as <c>IShodanClient.Directory</c>.
/// </summary>
public interface IQueryDirectoryService
{
	/// <summary>
	///     Lists search queries that users have saved to the Shodan directory (<c>GET /shodan/query</c>).
	/// </summary>
	/// <param name="page">The 1-based page number.</param>
	/// <param name="sort">Optional field to sort on (e.g. <c>votes</c>, <c>timestamp</c>).</param>
	/// <param name="order">Optional sort order (<c>asc</c> or <c>desc</c>).</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<SavedQueryResult> ListQueriesAsync(
		int page = 1,
		string? sort = null,
		string? order = null,
		CancellationToken cancellationToken = default);

	/// <summary>Searches the directory of saved search queries (<c>GET /shodan/query/search</c>).</summary>
	/// <param name="query">The text to search for in the directory of saved queries.</param>
	/// <param name="page">The 1-based page number.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<SavedQueryResult> SearchQueriesAsync(
		string query,
		int page = 1,
		CancellationToken cancellationToken = default);

	/// <summary>Lists the most popular tags across saved queries (<c>GET /shodan/query/tags</c>).</summary>
	/// <param name="size">The number of tags to return.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<IReadOnlyList<QueryTag>> ListTagsAsync(int size = 10, CancellationToken cancellationToken = default);
}
