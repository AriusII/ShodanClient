using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Utility;

/// <summary>REST implementation of <see cref="IUtilityRepository" />.</summary>
internal sealed class UtilityRepository(RestChannel channel) : IUtilityRepository
{
	public async Task<IReadOnlyDictionary<string, string>> GetHttpHeadersAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Utility.HttpHeaders();
		return await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.DictionaryStringString, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task<string> GetMyIpAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Utility.MyIp();
		return await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.String, cancellationToken)
			.ConfigureAwait(false);
	}
}
