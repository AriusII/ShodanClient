namespace ShodanClient.Domain.Notifiers;

/// <summary>Result of creating a notifier (<c>POST /notifier</c>).</summary>
public sealed record CreateNotifierResult
{
	/// <summary>Whether the notifier was created successfully (<c>success</c>).</summary>
	public bool Success { get; init; }

	/// <summary>The identifier assigned to the new notifier (<c>id</c>).</summary>
	public string? Id { get; init; }
}
