using ShodanClient.Domain.Scanning;

namespace ShodanClient.Application.Abstractions.Services;

/// <summary>
///     Requesting and monitoring on-demand scans of Shodan. Exposed on the client as
///     <c>IShodanClient.Scans</c>.
/// </summary>
public interface IScanService
{
	/// <summary>Lists the ports that Shodan crawls (<c>GET /shodan/ports</c>).</summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<IReadOnlyList<int>> GetCrawledPortsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	///     Lists the protocols available for scanning, keyed by protocol name with a human-readable
	///     description as the value (<c>GET /shodan/protocols</c>).
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<IReadOnlyDictionary<string, string>> GetProtocolsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	///     Requests an on-demand crawl of the given IPs/netblocks. Each IP consumes one scan credit
	///     (<c>POST /shodan/scan</c>).
	/// </summary>
	/// <param name="ipsOrNetblocks">The IP addresses or netblocks (CIDR) to scan.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<ScanSubmission> RequestScanAsync(
		IEnumerable<string> ipsOrNetblocks,
		CancellationToken cancellationToken = default);

	/// <summary>
	///     Requests an Internet-wide crawl for a specific port and protocol. Requires an
	///     Enterprise/researcher plan (<c>POST /shodan/scan/internet</c>).
	/// </summary>
	/// <param name="port">The port to scan.</param>
	/// <param name="protocol">The name of the protocol to use when scanning.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<ScanInternetResult> ScanInternetAsync(
		int port,
		string protocol,
		CancellationToken cancellationToken = default);

	/// <summary>Lists active on-demand scans (<c>GET /shodan/scans</c>).</summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<IReadOnlyList<ScanListEntry>> ListScansAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets the status of a previously submitted scan (<c>GET /shodan/scans/{id}</c>).</summary>
	/// <param name="scanId">The scan identifier returned when the scan was submitted.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<ScanStatus> GetScanStatusAsync(string scanId, CancellationToken cancellationToken = default);
}
