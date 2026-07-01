using ShodanClient.Domain.InternetDb;

namespace ShodanClient.Application.Abstractions.Services;

/// <summary>
///     Fast, key-less IP summaries from InternetDB. Exposed on the client as
///     <c>IShodanClient.InternetDb</c>. Unlike every other sub-client this one never requires an API
///     key — InternetDB is a free, weekly-updated dataset.
/// </summary>
public interface IInternetDbService
{
	/// <summary>Returns the open ports, CPEs, hostnames, tags and known vulnerabilities for an IP.</summary>
	/// <param name="ip">The IP address to look up.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<InternetDbHost> GetAsync(string ip, CancellationToken cancellationToken = default);
}
