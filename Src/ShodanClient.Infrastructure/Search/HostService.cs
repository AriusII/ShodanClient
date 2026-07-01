using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Application.Common;
using ShodanClient.Application.Search;
using ShodanClient.Domain.Search;

namespace ShodanClient.Infrastructure.Search;

/// <summary>Logic layer for host lookups. Validates input, then delegates to the repository.</summary>
internal sealed class HostService(IHostRepository repository) : IHostService
{
	public Task<Host> GetAsync(
		string ip,
		bool history = false,
		bool minify = false,
		CancellationToken cancellationToken = default)
	{
		Guard.ValidIpAddress(ip);
		return repository.GetAsync(new HostLookupQuery(ip, history, minify), cancellationToken);
	}
}
