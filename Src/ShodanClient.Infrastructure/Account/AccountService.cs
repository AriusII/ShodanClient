using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Domain.Account;

namespace ShodanClient.Infrastructure.Account;

/// <summary>Logic layer for inspecting the calling API key's account.</summary>
internal sealed class AccountService(IAccountRepository repository) : IAccountService
{
	public Task<AccountProfile> GetProfileAsync(CancellationToken cancellationToken = default) =>
		repository.GetProfileAsync(cancellationToken);
}
