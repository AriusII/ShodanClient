using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Domain.InternetDb;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.InternetDb.Mapping;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.InternetDb;

/// <summary>Implementation of <see cref="IInternetDbRepository" /> against the key-less InternetDB API.</summary>
internal sealed class InternetDbRepository(InternetDbChannel channel) : IInternetDbRepository
{
	public async Task<InternetDbHost> GetAsync(string ip, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.InternetDb.Host(ip);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.InternetDbResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}
}
