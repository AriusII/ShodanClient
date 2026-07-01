namespace ShodanClient.Domain.Alerts;

/// <summary>
///     A network alert configured to watch a set of IPs/netblocks for events, as returned by
///     <c>POST /shodan/alert</c>, <c>GET /shodan/alert/info</c> and <c>GET /shodan/alert/{id}/info</c>.
/// </summary>
public sealed record Alert
{
	/// <summary>The alert's unique identifier (<c>id</c>).</summary>
	public required string Id { get; init; }

	/// <summary>The alert's display name (<c>name</c>).</summary>
	public required string Name { get; init; }

	/// <summary>When the alert was created (<c>created</c>).</summary>
	public DateTimeOffset? Created { get; init; }

	/// <summary>Number of seconds the alert stays active; <c>0</c> means it never expires (<c>expires</c>).</summary>
	public int Expires { get; init; }

	/// <summary>The number of IPs covered by the alert (<c>size</c>).</summary>
	public int Size { get; init; }

	/// <summary>The criteria that trigger the alert (<c>filters</c>).</summary>
	public AlertFilters Filters { get; init; } = new();

	/// <summary>Triggers enabled on the alert, keyed by trigger name (<c>triggers</c>).</summary>
	public IReadOnlyDictionary<string, AlertTrigger> Triggers { get; init; } = new Dictionary<string, AlertTrigger>();

	/// <summary>Whether the alert currently has any triggers enabled (<c>has_triggers</c>).</summary>
	public bool HasTriggers { get; init; }

	/// <summary>The alert's absolute expiration timestamp, if any (<c>expiration</c>).</summary>
	public string? Expiration { get; init; }

	/// <summary>Notification services attached to the alert (<c>notifiers</c>).</summary>
	public IReadOnlyList<AlertNotifierRef> Notifiers { get; init; } = [];
}
