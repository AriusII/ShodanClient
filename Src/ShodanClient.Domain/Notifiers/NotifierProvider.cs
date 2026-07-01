namespace ShodanClient.Domain.Notifiers;

/// <summary>
///     A notification provider supported by Shodan (e.g. <c>email</c>, <c>slack</c>) together with the
///     arguments it requires when creating or editing a notifier (<c>GET /notifier/provider</c>).
/// </summary>
public sealed record NotifierProvider
{
	/// <summary>The provider's identifier, e.g. <c>slack</c> (the JSON object key).</summary>
	public required string Name { get; init; }

	/// <summary>The argument names required to configure the provider (<c>required</c>).</summary>
	public IReadOnlyList<string> Required { get; init; } = [];
}
