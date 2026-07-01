using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Application.Common;
using ShodanClient.Application.Directory;
using ShodanClient.Domain.Directory;

namespace ShodanClient.Infrastructure.Directory;

/// <summary>Logic layer for browsing and searching the public directory of saved Shodan queries.</summary>
internal sealed class DirectoryService(IDirectoryRepository repository) : IQueryDirectoryService
{
	public Task<SavedQueryResult> ListQueriesAsync(
		int page = 1,
		string? sort = null,
		string? order = null,
		CancellationToken cancellationToken = default)
	{
		Guard.AtLeast(page, 1);
		return repository.ListQueriesAsync(new ListSavedQueriesQuery(page, sort, order), cancellationToken);
	}

	public Task<SavedQueryResult> SearchQueriesAsync(
		string query,
		int page = 1,
		CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(query);
		Guard.AtLeast(page, 1);
		return repository.SearchQueriesAsync(new SearchSavedQueriesQuery(query, page), cancellationToken);
	}

	public Task<IReadOnlyList<QueryTag>> ListTagsAsync(int size = 10, CancellationToken cancellationToken = default)
	{
		Guard.AtLeast(size, 1);
		return repository.ListTagsAsync(new ListQueryTagsQuery(size), cancellationToken);
	}
}
