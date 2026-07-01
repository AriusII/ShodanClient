using ShodanClient.Application.Directory;
using ShodanClient.Domain.Directory;

namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to the directory family of endpoints on the REST API.</summary>
internal interface IDirectoryRepository
{
	/// <summary>Lists saved search queries (<c>GET /shodan/query</c>).</summary>
	Task<SavedQueryResult> ListQueriesAsync(ListSavedQueriesQuery query, CancellationToken cancellationToken);

	/// <summary>Searches the directory of saved search queries (<c>GET /shodan/query/search</c>).</summary>
	Task<SavedQueryResult> SearchQueriesAsync(SearchSavedQueriesQuery query, CancellationToken cancellationToken);

	/// <summary>Lists the most popular saved-query tags (<c>GET /shodan/query/tags</c>).</summary>
	Task<IReadOnlyList<QueryTag>> ListTagsAsync(ListQueryTagsQuery query, CancellationToken cancellationToken);
}
