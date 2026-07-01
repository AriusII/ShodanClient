using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Application.Common;
using ShodanClient.Application.Trends;
using ShodanClient.Domain.Trends;

namespace ShodanClient.Infrastructure.Trends;

/// <summary>
///     Logic layer for the Trends API. Validates queries, applies defaults and delegates to
///     <see cref="ITrendsRepository" />.
/// </summary>
internal sealed class TrendsService(ITrendsRepository repository) : ITrendsService
{
	public Task<TrendResult> SearchAsync(
		string query,
		string? facets = null,
		CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(query);
		return repository.SearchAsync(new TrendSearchQuery(query, facets), cancellationToken);
	}

	public Task<IReadOnlyList<string>> GetFiltersAsync(CancellationToken cancellationToken = default) =>
		repository.ListFiltersAsync(cancellationToken);

	public Task<IReadOnlyList<string>> GetFacetsAsync(CancellationToken cancellationToken = default) =>
		repository.ListFacetsAsync(cancellationToken);
}
