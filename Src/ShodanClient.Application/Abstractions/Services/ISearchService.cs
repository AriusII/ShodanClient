using ShodanClient.Domain.Search;

namespace ShodanClient.Application.Abstractions.Services;

/// <summary>
///     Searching Shodan and inspecting the search grammar. Exposed on the client as
///     <c>IShodanClient.Search</c>.
/// </summary>
public interface ISearchService
{
	/// <summary>
	///     Searches the Shodan banner database. Note that queries containing filters, or pages beyond
	///     the first, consume query credits.
	/// </summary>
	/// <param name="query">The Shodan search query.</param>
	/// <param name="facets">Optional comma-separated facets to summarize (e.g. <c>country,org</c>).</param>
	/// <param name="page">The 1-based page number (100 results per page).</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<SearchResult> SearchAsync(
		string query,
		string? facets = null,
		int page = 1,
		CancellationToken cancellationToken = default);

	/// <summary>
	///     Streams every match for a query, transparently paging through the result set. Each page
	///     after the first consumes a query credit.
	/// </summary>
	/// <param name="query">The Shodan search query.</param>
	/// <param name="startPage">The 1-based page to start from.</param>
	/// <param name="maxPages">An optional cap on the number of pages to fetch.</param>
	/// <param name="cancellationToken">A token to stop the enumeration.</param>
	IAsyncEnumerable<Banner> SearchAllAsync(
		string query,
		int startPage = 1,
		int? maxPages = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	///     Returns the total number of results for a query without returning banners or consuming
	///     query credits (<c>GET /shodan/host/count</c>).
	/// </summary>
	Task<CountResult> CountAsync(
		string query,
		string? facets = null,
		CancellationToken cancellationToken = default);

	/// <summary>Lists the facets that can be used to summarize search results.</summary>
	Task<IReadOnlyList<string>> GetFacetsAsync(CancellationToken cancellationToken = default);

	/// <summary>Lists the filters that can be used in a search query.</summary>
	Task<IReadOnlyList<string>> GetFiltersAsync(CancellationToken cancellationToken = default);

	/// <summary>Parses a search query into the filters and free-text it resolves to.</summary>
	Task<QueryTokenParse> ParseQueryAsync(string query, CancellationToken cancellationToken = default);
}
