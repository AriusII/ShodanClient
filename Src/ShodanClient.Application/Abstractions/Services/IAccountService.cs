using ShodanClient.Domain.Account;

namespace ShodanClient.Application.Abstractions.Services;

/// <summary>
///     Inspecting the calling API key's account. Exposed on the client as <c>IShodanClient.Account</c>.
/// </summary>
public interface IAccountService
{
	/// <summary>
	///     Returns the account's membership status, remaining query credits and display name.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<AccountProfile> GetProfileAsync(CancellationToken cancellationToken = default);
}
