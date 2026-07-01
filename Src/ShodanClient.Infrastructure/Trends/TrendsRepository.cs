using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Trends;
using ShodanClient.Domain.Trends;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Serialization;
using ShodanClient.Infrastructure.Trends.Mapping;

namespace ShodanClient.Infrastructure.Trends;

/// <summary>REST implementation of <see cref="ITrendsRepository" /> against the Trends API.</summary>
internal sealed class TrendsRepository(TrendsChannel channel) : ITrendsRepository
{
	public async Task<TrendResult> SearchAsync(TrendSearchQuery query, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Trends.Search(query.Query, query.Facets);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.TrendSearchResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<IReadOnlyList<string>> ListFiltersAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Trends.Filters();
		return await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.StringArray, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task<IReadOnlyList<string>> ListFacetsAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Trends.Facets();
		return await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.StringArray, cancellationToken)
			.ConfigureAwait(false);
	}
}
