namespace ShodanClient.Domain.Organization;

/// <summary>
///     An Enterprise account's organization, as returned by <c>GET /org</c>: its members, the domains
///     it claims and its branding.
/// </summary>
public sealed record Organization
{
	/// <summary>The organization's unique identifier, if assigned (<c>id</c>).</summary>
	public string? Id { get; init; }

	/// <summary>The organization's display name (<c>name</c>).</summary>
	public string? Name { get; init; }

	/// <summary>When the organization was created (<c>created</c>).</summary>
	public DateTimeOffset? Created { get; init; }

	/// <summary>The organization's administrators (<c>admins</c>).</summary>
	public IReadOnlyList<OrganizationMember> Admins { get; init; } = [];

	/// <summary>Every member of the organization (<c>members</c>).</summary>
	public IReadOnlyList<OrganizationMember> Members { get; init; } = [];

	/// <summary>The Shodan plan the organization's members are upgraded to, if any (<c>upgrade_type</c>).</summary>
	public string? UpgradeType { get; init; }

	/// <summary>The domains claimed by the organization (<c>domains</c>).</summary>
	public IReadOnlyList<string> Domains { get; init; } = [];

	/// <summary>Whether the organization has a custom logo configured (<c>logo</c>).</summary>
	public bool Logo { get; init; }
}
