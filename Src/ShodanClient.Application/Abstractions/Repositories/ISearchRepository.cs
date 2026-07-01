using ShodanClient.Application.Search;
using ShodanClient.Domain.Search;

namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to the search family of endpoints on the REST API.</summary>
internal interface ISearchRepository
{
	/// <summary>Runs a search (<c>GET /shodan/host/search</c>).</summary>
	Task<SearchResult> SearchAsync(HostSearchQuery query, CancellationToken cancellationToken);

	/// <summary>Runs a count-only search (<c>GET /shodan/host/count</c>).</summary>
	Task<CountResult> CountAsync(HostCountQuery query, CancellationToken cancellationToken);

	/// <summary>Lists the facets available for searching (<c>GET /shodan/host/search/facets</c>).</summary>
	Task<IReadOnlyList<string>> ListFacetsAsync(CancellationToken cancellationToken);

	/// <summary>Lists the filters available for searching (<c>GET /shodan/host/search/filters</c>).</summary>
	Task<IReadOnlyList<string>> ListFiltersAsync(CancellationToken cancellationToken);

	/// <summary>Parses a query into its tokens (<c>GET /shodan/host/search/tokens</c>).</summary>
	Task<QueryTokenParse> ParseQueryAsync(QueryTokenRequest request, CancellationToken cancellationToken);
}
