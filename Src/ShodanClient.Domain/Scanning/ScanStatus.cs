namespace ShodanClient.Domain.Scanning;

/// <summary>
///     The current state of a previously submitted on-demand scan (<c>GET /shodan/scans/{id}</c>).
/// </summary>
public sealed record ScanStatus
{
	/// <summary>Unique identifier for the scan (<c>id</c>).</summary>
	public required string Id { get; init; }

	/// <summary>
	///     Processing state of the scan (<c>status</c>): <c>SUBMITTING</c>, <c>QUEUE</c>,
	///     <c>PROCESSING</c> or <c>DONE</c>.
	/// </summary>
	public required string Status { get; init; }

	/// <summary>When the scan was created (<c>created</c>).</summary>
	public DateTimeOffset? Created { get; init; }

	/// <summary>Number of IPs included in the scan (<c>count</c>).</summary>
	public int Count { get; init; }
}
