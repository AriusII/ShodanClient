namespace ShodanClient.Domain.Notifiers;

/// <summary>
///     A configured notification channel that alerts can push matches to (<c>GET /notifier</c>,
///     <c>GET /notifier/{id}</c>).
/// </summary>
public sealed record Notifier
{
	/// <summary>Unique identifier of the notifier (<c>id</c>).</summary>
	public required string Id { get; init; }

	/// <summary>
	///     The notification provider, e.g. <c>email</c>, <c>slack</c>, <c>telegram</c>, <c>webhook</c>
	///     (<c>provider</c>).
	/// </summary>
	public required string Provider { get; init; }

	/// <summary>Human-readable description of the notifier (<c>description</c>).</summary>
	public string? Description { get; init; }

	/// <summary>Provider-specific arguments, e.g. <c>{ "to": "..." }</c> (<c>args</c>).</summary>
	public IReadOnlyDictionary<string, string> Args { get; init; } = new Dictionary<string, string>();
}
