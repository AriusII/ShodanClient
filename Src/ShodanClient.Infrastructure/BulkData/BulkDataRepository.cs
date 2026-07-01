using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Domain.BulkData;
using ShodanClient.Infrastructure.BulkData.Mapping;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.BulkData;

/// <summary>REST implementation of <see cref="IBulkDataRepository" />.</summary>
internal sealed class BulkDataRepository(RestChannel channel) : IBulkDataRepository
{
	public async Task<IReadOnlyList<Dataset>> ListDatasetsAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.BulkData.Datasets();
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.DatasetDtoArray, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<IReadOnlyList<DatasetFile>> ListFilesAsync(string dataset, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.BulkData.Files(dataset);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.DatasetFileDtoArray, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}
}
