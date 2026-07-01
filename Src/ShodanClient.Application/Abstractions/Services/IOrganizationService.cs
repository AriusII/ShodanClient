namespace ShodanClient.Application.Abstractions.Services;

// NOTE: the domain type is referenced by its fully qualified name below rather than a `using` +
// bare `Organization`, because this file's enclosing namespace chain passes through
// `ShodanClient.Application`, which also has a nested `Organization` namespace (the query-DTO
// home); an unqualified `Organization` would bind to that namespace and fail with CS0118.

/// <summary>
///     Inspecting and managing the calling account's Enterprise organization. Exposed on the client as
///     <c>IShodanClient.Organization</c>.
/// </summary>
public interface IOrganizationService
{
	/// <summary>Fetches the organization's profile, member list and claimed domains (<c>GET /org</c>).</summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<Domain.Organization.Organization> GetAsync(CancellationToken cancellationToken = default);

	/// <summary>
	///     Adds a user to the organization, or upgrades them if they are already a member
	///     (<c>PUT /org/member/{user}</c>).
	/// </summary>
	/// <param name="user">The username or email address of the member to add.</param>
	/// <param name="notify">Whether to notify the user by email that they were added.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<bool> AddMemberAsync(string user, bool notify = false, CancellationToken cancellationToken = default);

	/// <summary>
	///     Removes a user from the organization, or downgrades them if it can't remove them
	///     (<c>DELETE /org/member/{user}</c>).
	/// </summary>
	/// <param name="user">The username or email address of the member to remove.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<bool> RemoveMemberAsync(string user, CancellationToken cancellationToken = default);
}
