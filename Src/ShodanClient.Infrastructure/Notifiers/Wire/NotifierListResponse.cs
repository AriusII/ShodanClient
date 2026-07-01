using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Notifiers.Wire;

/// <summary>Wire shape of the <c>GET /notifier</c> response.</summary>
internal sealed class NotifierListResponse
{
	public long Total { get; set; }

	public NotifierDto[]? Matches { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
