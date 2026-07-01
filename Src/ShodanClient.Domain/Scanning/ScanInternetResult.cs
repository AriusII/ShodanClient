namespace ShodanClient.Domain.Scanning;

/// <summary>
///     Acknowledgement returned when an Internet-wide crawl of a port/protocol is requested
///     (<c>POST /shodan/scan/internet</c>).
/// </summary>
public sealed record ScanInternetResult
{
	/// <summary>Unique identifier for the submitted scan (<c>id</c>).</summary>
	public required string Id { get; init; }
}
