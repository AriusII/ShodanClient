using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Notifiers.Wire;

/// <summary>Wire shape of a single notifier (list element and the <c>GET /notifier/{id}</c> response).</summary>
internal sealed class NotifierDto
{
	public string? Id { get; set; }

	public string? Provider { get; set; }

	public string? Description { get; set; }

	public Dictionary<string, string>? Args { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
