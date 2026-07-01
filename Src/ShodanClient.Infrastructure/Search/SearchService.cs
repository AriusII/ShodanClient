using System.Runtime.CompilerServices;
using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Application.Common;
using ShodanClient.Application.Search;
using ShodanClient.Domain.Search;

namespace ShodanClient.Infrastructure.Search;

/// <summary>
///     Logic layer for searching Shodan. Validates queries, applies defaults and owns the auto-paging
///     enumeration used by <see cref="SearchAllAsync" />.
/// </summary>
internal sealed class SearchService(ISearchRepository repository) : ISearchService
{
	private const int PageSize = 100;

	public Task<SearchResult> SearchAsync(
		string query,
		string? facets = null,
		int page = 1,
		CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(query);
		Guard.AtLeast(page, 1);
		return repository.SearchAsync(new HostSearchQuery(query, facets, page), cancellationToken);
	}

	public async IAsyncEnumerable<Banner> SearchAllAsync(
		string query,
		int startPage = 1,
		int? maxPages = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(query);
		Guard.AtLeast(startPage, 1);

		var page = startPage;
		var pagesFetched = 0;

		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var result = await repository
				.SearchAsync(new HostSearchQuery(query, null, page), cancellationToken)
				.ConfigureAwait(false);

			if (result.Matches.Count == 0)
			{
				yield break;
			}

			foreach (var banner in result.Matches)
			{
				yield return banner;
			}

			pagesFetched++;
			if (maxPages is { } max && pagesFetched >= max)
			{
				yield break;
			}

			// Stop when the last page was short or we've covered the reported total.
			if (result.Matches.Count < PageSize || (long)page * PageSize >= result.Total)
			{
				yield break;
			}

			page++;
		}
	}

	public Task<CountResult> CountAsync(
		string query,
		string? facets = null,
		CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(query);
		return repository.CountAsync(new HostCountQuery(query, facets), cancellationToken);
	}

	public Task<IReadOnlyList<string>> GetFacetsAsync(CancellationToken cancellationToken = default) =>
		repository.ListFacetsAsync(cancellationToken);

	public Task<IReadOnlyList<string>> GetFiltersAsync(CancellationToken cancellationToken = default) =>
		repository.ListFiltersAsync(cancellationToken);

	public Task<QueryTokenParse> ParseQueryAsync(string query, CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(query);
		return repository.ParseQueryAsync(new QueryTokenRequest(query), cancellationToken);
	}
}
