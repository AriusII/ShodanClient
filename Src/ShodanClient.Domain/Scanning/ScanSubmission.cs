namespace ShodanClient.Domain.Scanning;

/// <summary>
///     Acknowledgement returned when a crawl of specific IPs/netblocks is requested
///     (<c>POST /shodan/scan</c>).
/// </summary>
public sealed record ScanSubmission
{
	/// <summary>Unique identifier for the submitted scan (<c>id</c>).</summary>
	public required string Id { get; init; }

	/// <summary>Number of IPs that were submitted for scanning (<c>count</c>).</summary>
	public int Count { get; init; }

	/// <summary>Scan credits remaining after this request (<c>credits_left</c>).</summary>
	public int CreditsLeft { get; init; }
}
