using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Organization.Wire;

/// <summary>Wire shape of the <c>GET /org</c> response.</summary>
internal sealed class OrganizationResponse
{
	public string? Id { get; set; }

	public string? Name { get; set; }

	public string? Created { get; set; }

	public OrganizationMemberDto[]? Admins { get; set; }

	public OrganizationMemberDto[]? Members { get; set; }

	public string? UpgradeType { get; set; }

	public string[]? Domains { get; set; }

	public bool Logo { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>Wire shape of a single entry in the <c>admins</c>/<c>members</c> arrays.</summary>
internal sealed class OrganizationMemberDto
{
	public string? Username { get; set; }

	public string? Email { get; set; }
}
