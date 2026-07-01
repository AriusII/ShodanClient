using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Directory;
using ShodanClient.Domain.Directory;
using ShodanClient.Infrastructure.Directory.Mapping;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Directory;

/// <summary>REST implementation of <see cref="IDirectoryRepository" />.</summary>
internal sealed class DirectoryRepository(RestChannel channel) : IDirectoryRepository
{
	public async Task<SavedQueryResult> ListQueriesAsync(ListSavedQueriesQuery query,
		CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Directory.ListQueries(query.Page, query.Sort, query.Order);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.SavedQueryResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<SavedQueryResult> SearchQueriesAsync(SearchSavedQueriesQuery query,
		CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Directory.SearchQueries(query.Query, query.Page);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.SavedQueryResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<IReadOnlyList<QueryTag>> ListTagsAsync(ListQueryTagsQuery query,
		CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Directory.Tags(query.Size);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.QueryTagsResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}
}
