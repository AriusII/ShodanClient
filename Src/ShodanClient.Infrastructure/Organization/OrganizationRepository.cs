using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Organization;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Organization.Mapping;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Organization;

// NOTE: the domain type is referenced by its fully qualified name below rather than a `using` +
// bare `Organization`, because this file's own namespace IS `ShodanClient.Infrastructure.Organization`;
// an unqualified `Organization` would bind to that enclosing namespace and fail with CS0118.

/// <summary>REST implementation of <see cref="IOrganizationRepository" />.</summary>
internal sealed class OrganizationRepository(RestChannel channel) : IOrganizationRepository
{
	public async Task<Domain.Organization.Organization> GetAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Organization.Org();
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.OrganizationResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<bool> AddMemberAsync(AddOrganizationMemberQuery query, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Organization.AddMember(query.User, query.Notify);
		return await channel.SendForSuccessAsync(route, null, cancellationToken).ConfigureAwait(false);
	}

	public async Task<bool> RemoveMemberAsync(string user, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Organization.RemoveMember(user);
		return await channel.SendForSuccessAsync(route, null, cancellationToken).ConfigureAwait(false);
	}
}
