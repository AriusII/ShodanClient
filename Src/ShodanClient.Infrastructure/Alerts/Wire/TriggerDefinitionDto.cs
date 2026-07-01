using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Alerts.Wire;

/// <summary>Wire shape of a single entry in the <c>GET /shodan/alert/triggers</c> response array.</summary>
internal sealed class TriggerDefinitionDto
{
	public string? Name { get; set; }

	public string? Rule { get; set; }

	public string? Description { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
