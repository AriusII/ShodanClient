using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.ApiStatus.Wire;

/// <summary>Wire shape of the <c>GET /api-info</c> response.</summary>
internal sealed class ApiInfoResponse
{
	public int ScanCredits { get; set; }

	public int QueryCredits { get; set; }

	public int? MonitoredIps { get; set; }

	public string? Plan { get; set; }

	public bool Https { get; set; }

	public bool Telnet { get; set; }

	public bool Unlocked { get; set; }

	public int UnlockedLeft { get; set; }

	public UsageLimitsDto? UsageLimits { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>Wire shape of the <c>usage_limits</c> block.</summary>
internal sealed class UsageLimitsDto
{
	public int ScanCredits { get; set; }

	public int QueryCredits { get; set; }

	public int? MonitoredIps { get; set; }
}
