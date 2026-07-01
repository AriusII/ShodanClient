using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Notifiers.Wire;

/// <summary>Wire shape of the <c>POST /notifier</c> response.</summary>
internal sealed class CreateNotifierResponse
{
	public bool Success { get; set; }

	public string? Id { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
