using ShodanClient.Application.Common;

namespace ShodanClient.Application.Organization;

/// <summary>
///     Parameters for adding or upgrading an organization member (<c>PUT /org/member/{user}</c>).
/// </summary>
/// <param name="User">The username or email address of the member to add.</param>
/// <param name="Notify">Whether to notify the user by email that they were added.</param>
internal sealed record AddOrganizationMemberQuery(string User, bool Notify) : IShodanQuery;
