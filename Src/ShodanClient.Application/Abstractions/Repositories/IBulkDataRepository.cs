using ShodanClient.Domain.BulkData;

namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to the bulk-data family of endpoints on the REST API.</summary>
internal interface IBulkDataRepository
{
	/// <summary>Lists the datasets available for bulk download (<c>GET /shodan/data</c>).</summary>
	Task<IReadOnlyList<Dataset>> ListDatasetsAsync(CancellationToken cancellationToken);

	/// <summary>Lists the files within a dataset (<c>GET /shodan/data/{dataset}</c>).</summary>
	Task<IReadOnlyList<DatasetFile>> ListFilesAsync(string dataset, CancellationToken cancellationToken);
}
