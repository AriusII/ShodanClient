namespace ShodanClient.Domain.Directory;

/// <summary>
///     A search query that a Shodan user has saved to the public directory, together with its
///     community rating and descriptive metadata.
/// </summary>
public sealed record SavedQuery
{
	/// <summary>The saved Shodan search query (<c>query</c>).</summary>
	public required string Query { get; init; }

	/// <summary>Number of times other users have voted for the query (<c>votes</c>).</summary>
	public int Votes { get; init; }

	/// <summary>Human-readable title given to the query (<c>title</c>).</summary>
	public string? Title { get; init; }

	/// <summary>Description of what the query returns (<c>description</c>).</summary>
	public string? Description { get; init; }

	/// <summary>When the query was saved to the directory (<c>timestamp</c>).</summary>
	public DateTimeOffset? Timestamp { get; init; }

	/// <summary>Tags associated with the query (<c>tags</c>).</summary>
	public IReadOnlyList<string> Tags { get; init; } = [];
}
