using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Domain.Account;
using ShodanClient.Infrastructure.Account.Mapping;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Account;

/// <summary>REST implementation of <see cref="IAccountRepository" />.</summary>
internal sealed class AccountRepository(RestChannel channel) : IAccountRepository
{
	public async Task<AccountProfile> GetProfileAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Account.Profile();
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.AccountProfileResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}
}
