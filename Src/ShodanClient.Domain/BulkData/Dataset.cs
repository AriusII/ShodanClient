namespace ShodanClient.Domain.BulkData;

/// <summary>
///     A bulk-data dataset made available to Enterprise accounts (<c>GET /shodan/data</c>).
/// </summary>
public sealed record Dataset
{
	/// <summary>How often the dataset is refreshed, e.g. <c>monthly</c>, <c>daily</c> (<c>scope</c>).</summary>
	public string? Scope { get; init; }

	/// <summary>The dataset's short name, used to look up its files (<c>name</c>).</summary>
	public required string Name { get; init; }

	/// <summary>A human-readable description of the dataset's contents (<c>description</c>).</summary>
	public string? Description { get; init; }
}
