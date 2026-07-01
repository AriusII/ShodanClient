namespace ShodanClient.Domain.Organization;

/// <summary>A single admin or member of an <see cref="Organization" />.</summary>
public sealed record OrganizationMember
{
	/// <summary>The member's Shodan username (<c>username</c>).</summary>
	public required string Username { get; init; }

	/// <summary>The member's email address (<c>email</c>).</summary>
	public string? Email { get; init; }
}
