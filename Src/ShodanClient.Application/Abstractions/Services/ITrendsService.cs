using ShodanClient.Domain.Trends;

namespace ShodanClient.Application.Abstractions.Services;

/// <summary>
///     Inspecting historical, month-by-month search trends on the Trends API. Exposed on the client
///     as <c>IShodanClient.Trends</c>.
/// </summary>
public interface ITrendsService
{
	/// <summary>
	///     Runs a Trends search, returning a month-by-month historical breakdown of matches for a
	///     query rather than the current banners themselves.
	/// </summary>
	/// <param name="query">The Shodan search query.</param>
	/// <param name="facets">Optional comma-separated facets to summarize month by month (e.g. <c>country,org</c>).</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<TrendResult> SearchAsync(
		string query,
		string? facets = null,
		CancellationToken cancellationToken = default);

	/// <summary>Lists the filters that can be used in a Trends search query.</summary>
	Task<IReadOnlyList<string>> GetFiltersAsync(CancellationToken cancellationToken = default);

	/// <summary>Lists the facets that can be used to summarize a Trends search.</summary>
	Task<IReadOnlyList<string>> GetFacetsAsync(CancellationToken cancellationToken = default);
}
