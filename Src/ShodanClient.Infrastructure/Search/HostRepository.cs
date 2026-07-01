using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Search;
using ShodanClient.Domain.Search;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Search.Mapping;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Search;

/// <summary>REST implementation of <see cref="IHostRepository" />.</summary>
internal sealed class HostRepository(RestChannel channel) : IHostRepository
{
	public async Task<Host> GetAsync(HostLookupQuery query, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Search.Host(query.Ip, query.History, query.Minify);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.HostResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}
}
