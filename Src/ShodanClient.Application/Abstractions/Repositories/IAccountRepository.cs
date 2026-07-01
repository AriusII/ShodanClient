using ShodanClient.Domain.Account;

namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to the account family of endpoints on the REST API.</summary>
internal interface IAccountRepository
{
	/// <summary>Fetches the calling API key's account profile (<c>GET /account/profile</c>).</summary>
	Task<AccountProfile> GetProfileAsync(CancellationToken cancellationToken);
}
