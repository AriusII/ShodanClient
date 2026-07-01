using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Account.Wire;

/// <summary>Wire shape of the <c>GET /account/profile</c> response.</summary>
internal sealed class AccountProfileResponse
{
	public bool Member { get; set; }

	public int Credits { get; set; }

	public string? DisplayName { get; set; }

	public string? Created { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
