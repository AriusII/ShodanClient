using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Alerts.Wire;

/// <summary>Wire shape of the JSON body sent to <c>POST /shodan/alert</c> (create) requests.</summary>
internal sealed class CreateAlertPayload
{
	[JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

	[JsonPropertyName("filters")] public AlertFilterPayload Filters { get; set; } = new();

	[JsonPropertyName("expires")] public int Expires { get; set; }
}

/// <summary>Wire shape of the <c>filters</c> object in alert create/update JSON bodies.</summary>
internal sealed class AlertFilterPayload
{
	[JsonPropertyName("ip")] public string[] Ip { get; set; } = [];
}
