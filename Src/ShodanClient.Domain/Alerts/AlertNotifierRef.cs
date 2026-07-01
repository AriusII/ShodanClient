namespace ShodanClient.Domain.Alerts;

/// <summary>A notification service attached to an alert (an <see cref="Alert.Notifiers" /> entry).</summary>
public sealed record AlertNotifierRef
{
	/// <summary>The notifier's unique identifier (<c>id</c>).</summary>
	public required string Id { get; init; }

	/// <summary>The notifier's provider, e.g. <c>email</c>, <c>slack</c> (<c>provider</c>).</summary>
	public string? Provider { get; init; }
}
