using ShodanClient.Application.Organization;

namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to the organization-management family of endpoints on the REST API.</summary>
internal interface IOrganizationRepository
{
	// NOTE: the domain type is referenced by its fully qualified name rather than a `using` +
	// bare `Organization`, because `ShodanClient.Application.Organization` (this file's sibling
	// namespace, imported above for the query DTO) shares its final segment with the domain type
	// name; an unqualified `Organization` would bind to that namespace and fail with CS0118.

	/// <summary>Fetches the calling account's organization (<c>GET /org</c>).</summary>
	Task<Domain.Organization.Organization> GetAsync(CancellationToken cancellationToken);

	/// <summary>Adds or upgrades a member of the organization (<c>PUT /org/member/{user}</c>).</summary>
	Task<bool> AddMemberAsync(AddOrganizationMemberQuery query, CancellationToken cancellationToken);

	/// <summary>Removes or downgrades a member of the organization (<c>DELETE /org/member/{user}</c>).</summary>
	Task<bool> RemoveMemberAsync(string user, CancellationToken cancellationToken);
}
