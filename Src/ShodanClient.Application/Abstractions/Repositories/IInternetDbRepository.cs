using ShodanClient.Domain.InternetDb;

namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to the key-less InternetDB API.</summary>
internal interface IInternetDbRepository
{
	/// <summary>Fetches the InternetDB summary for an IP (<c>GET /{ip}</c>).</summary>
	Task<InternetDbHost> GetAsync(string ip, CancellationToken cancellationToken);
}
