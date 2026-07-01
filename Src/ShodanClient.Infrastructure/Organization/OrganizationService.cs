using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Application.Common;
using ShodanClient.Application.Organization;

namespace ShodanClient.Infrastructure.Organization;

// NOTE: the domain type is referenced by its fully qualified name below rather than a `using` +
// bare `Organization`, because this file's own namespace IS `ShodanClient.Infrastructure.Organization`;
// an unqualified `Organization` would bind to that enclosing namespace and fail with CS0118.

/// <summary>Logic layer for inspecting and managing the calling account's Enterprise organization.</summary>
internal sealed class OrganizationService(IOrganizationRepository repository) : IOrganizationService
{
	public Task<Domain.Organization.Organization> GetAsync(CancellationToken cancellationToken = default) =>
		repository.GetAsync(cancellationToken);

	public Task<bool> AddMemberAsync(
		string user,
		bool notify = false,
		CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(user);
		return repository.AddMemberAsync(new AddOrganizationMemberQuery(user, notify), cancellationToken);
	}

	public Task<bool> RemoveMemberAsync(string user, CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(user);
		return repository.RemoveMemberAsync(user, cancellationToken);
	}
}
