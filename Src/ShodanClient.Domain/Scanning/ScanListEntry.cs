namespace ShodanClient.Domain.Scanning;

/// <summary>A single entry in the list of active on-demand scans (<c>GET /shodan/scans</c>).</summary>
public sealed record ScanListEntry
{
	/// <summary>Unique identifier for the scan (<c>id</c>).</summary>
	public required string Id { get; init; }

	/// <summary>When the scan was created (<c>created</c>).</summary>
	public DateTimeOffset? Created { get; init; }

	/// <summary>
	///     Processing state of the scan (<c>status</c>): <c>SUBMITTING</c>, <c>QUEUE</c>,
	///     <c>PROCESSING</c> or <c>DONE</c>.
	/// </summary>
	public required string Status { get; init; }

	/// <summary>Number of IPs included in the scan (<c>size</c>).</summary>
	public int Size { get; init; }

	/// <summary>Scan credits remaining after this scan (<c>credits_left</c>).</summary>
	public int CreditsLeft { get; init; }
}
