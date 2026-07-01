using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Search;
using ShodanClient.Domain.Search;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Search.Mapping;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Search;

/// <summary>REST implementation of <see cref="ISearchRepository" />.</summary>
internal sealed class SearchRepository(RestChannel channel) : ISearchRepository
{
	public async Task<SearchResult> SearchAsync(HostSearchQuery query, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Search.SearchHosts(query.Query, query.Facets, query.Page);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.SearchResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<CountResult> CountAsync(HostCountQuery query, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Search.Count(query.Query, query.Facets);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.CountResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<IReadOnlyList<string>> ListFacetsAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Search.Facets();
		return await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.StringArray, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task<IReadOnlyList<string>> ListFiltersAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Search.Filters();
		return await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.StringArray, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task<QueryTokenParse> ParseQueryAsync(QueryTokenRequest request, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Search.Tokens(request.Query);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.TokenParseResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}
}
