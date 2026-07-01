using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Application.Common;
using ShodanClient.Domain.InternetDb;

namespace ShodanClient.Infrastructure.InternetDb;

/// <summary>Logic layer for InternetDB lookups. Validates the IP, then delegates to the repository.</summary>
internal sealed class InternetDbService(IInternetDbRepository repository) : IInternetDbService
{
	public Task<InternetDbHost> GetAsync(string ip, CancellationToken cancellationToken = default)
	{
		Guard.ValidIpAddress(ip);
		return repository.GetAsync(ip, cancellationToken);
	}
}
