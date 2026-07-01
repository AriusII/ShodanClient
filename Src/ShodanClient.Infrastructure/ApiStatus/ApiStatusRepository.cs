using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Domain.ApiStatus;
using ShodanClient.Infrastructure.ApiStatus.Mapping;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.ApiStatus;

/// <summary>REST implementation of <see cref="IApiStatusRepository" />.</summary>
internal sealed class ApiStatusRepository(RestChannel channel) : IApiStatusRepository
{
	public async Task<ApiInfo> GetAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.ApiStatus.Info();
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.ApiInfoResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}
}
