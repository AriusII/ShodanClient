using ShodanClient.Application.Search;
using ShodanClient.Domain.Search;

namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to host lookups on the REST API.</summary>
internal interface IHostRepository
{
	/// <summary>Fetches all services found on a host (<c>GET /shodan/host/{ip}</c>).</summary>
	Task<Host> GetAsync(HostLookupQuery query, CancellationToken cancellationToken);
}
