using ShodanClient.Domain.BulkData;

namespace ShodanClient.Application.Abstractions.Services;

/// <summary>
///     Browsing Shodan's Enterprise bulk-data datasets. Exposed on the client as
///     <c>IShodanClient.BulkData</c>.
/// </summary>
public interface IBulkDataService
{
	/// <summary>Lists the datasets available for bulk download (<c>GET /shodan/data</c>).</summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<IReadOnlyList<Dataset>> ListDatasetsAsync(CancellationToken cancellationToken = default);

	/// <summary>Lists the files within a dataset (<c>GET /shodan/data/{dataset}</c>).</summary>
	/// <param name="dataset">The dataset's short name (see <see cref="ListDatasetsAsync" />).</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<IReadOnlyList<DatasetFile>> ListFilesAsync(string dataset, CancellationToken cancellationToken = default);
}
