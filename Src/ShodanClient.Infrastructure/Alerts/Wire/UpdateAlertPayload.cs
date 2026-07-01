using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Alerts.Wire;

/// <summary>Wire shape of the JSON body sent to <c>POST /shodan/alert/{id}</c> (edit) requests.</summary>
internal sealed class UpdateAlertPayload
{
	[JsonPropertyName("filters")] public AlertFilterPayload Filters { get; set; } = new();
}
