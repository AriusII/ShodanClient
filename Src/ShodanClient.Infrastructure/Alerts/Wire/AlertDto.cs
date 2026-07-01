using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Alerts.Wire;

/// <summary>
///     Wire shape of an alert, as returned by <c>POST /shodan/alert</c>, <c>GET /shodan/alert/info</c>
///     (array element) and <c>GET /shodan/alert/{id}/info</c>.
/// </summary>
internal sealed class AlertDto
{
	public string? Id { get; set; }

	public string? Name { get; set; }

	public string? Created { get; set; }

	public int Expires { get; set; }

	public int Size { get; set; }

	public AlertFiltersDto? Filters { get; set; }

	public Dictionary<string, AlertTriggerDto>? Triggers { get; set; }

	public bool HasTriggers { get; set; }

	public string? Expiration { get; set; }

	public AlertNotifierRefDto[]? Notifiers { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>Wire shape of the <c>filters</c> block on an alert.</summary>
internal sealed class AlertFiltersDto
{
	public string[]? Ip { get; set; }
}

/// <summary>Wire shape of a single entry in an alert's <c>triggers</c> map.</summary>
internal sealed class AlertTriggerDto
{
	public string? Rule { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>Wire shape of a single entry in an alert's <c>notifiers</c> array.</summary>
internal sealed class AlertNotifierRefDto
{
	public string? Id { get; set; }

	public string? Provider { get; set; }
}
