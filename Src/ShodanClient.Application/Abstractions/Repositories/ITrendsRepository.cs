using ShodanClient.Application.Trends;
using ShodanClient.Domain.Trends;

namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to the Trends family of endpoints (<c>trends.shodan.io</c>).</summary>
internal interface ITrendsRepository
{
	/// <summary>Runs a Trends search (<c>GET /api/v1/search</c>).</summary>
	Task<TrendResult> SearchAsync(TrendSearchQuery query, CancellationToken cancellationToken);

	/// <summary>Lists the filters available for a Trends search (<c>GET /api/v1/search/filters</c>).</summary>
	Task<IReadOnlyList<string>> ListFiltersAsync(CancellationToken cancellationToken);

	/// <summary>Lists the facets available for a Trends search (<c>GET /api/v1/search/facets</c>).</summary>
	Task<IReadOnlyList<string>> ListFacetsAsync(CancellationToken cancellationToken);
}
