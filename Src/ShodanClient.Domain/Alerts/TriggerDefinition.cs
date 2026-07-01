namespace ShodanClient.Domain.Alerts;

/// <summary>A trigger available to enable on an alert, as catalogued by <c>GET /shodan/alert/triggers</c>.</summary>
public sealed record TriggerDefinition
{
	/// <summary>The trigger's name, e.g. <c>malware</c>, <c>vulnerable</c> (the map key).</summary>
	public required string Name { get; init; }

	/// <summary>The trigger's rule expression, if any (<c>rule</c>).</summary>
	public string? Rule { get; init; }

	/// <summary>Human-readable description of the trigger (<c>description</c>).</summary>
	public string? Description { get; init; }
}
