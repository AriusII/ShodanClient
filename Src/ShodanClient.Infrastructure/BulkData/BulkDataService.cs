using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Application.Common;
using ShodanClient.Domain.BulkData;

namespace ShodanClient.Infrastructure.BulkData;

/// <summary>Logic layer for browsing Shodan's Enterprise bulk-data datasets.</summary>
internal sealed class BulkDataService(IBulkDataRepository repository) : IBulkDataService
{
	public Task<IReadOnlyList<Dataset>> ListDatasetsAsync(CancellationToken cancellationToken = default) =>
		repository.ListDatasetsAsync(cancellationToken);

	public Task<IReadOnlyList<DatasetFile>> ListFilesAsync(
		string dataset,
		CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(dataset);
		return repository.ListFilesAsync(dataset, cancellationToken);
	}
}
