using ShodanClient.Application.Scanning;
using ShodanClient.Domain.Scanning;

namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to the on-demand scanning family of endpoints on the REST API.</summary>
internal interface IScanRepository
{
	/// <summary>Lists the ports that Shodan crawls (<c>GET /shodan/ports</c>).</summary>
	Task<IReadOnlyList<int>> GetCrawledPortsAsync(CancellationToken cancellationToken);

	/// <summary>Lists the protocols available for scanning, keyed by protocol name (<c>GET /shodan/protocols</c>).</summary>
	Task<IReadOnlyDictionary<string, string>> GetProtocolsAsync(CancellationToken cancellationToken);

	/// <summary>Requests an on-demand crawl of specific IPs/netblocks (<c>POST /shodan/scan</c>).</summary>
	Task<ScanSubmission> RequestScanAsync(ScanRequestQuery query, CancellationToken cancellationToken);

	/// <summary>Requests an Internet-wide crawl of a port/protocol (<c>POST /shodan/scan/internet</c>).</summary>
	Task<ScanInternetResult> ScanInternetAsync(ScanInternetQuery query, CancellationToken cancellationToken);

	/// <summary>Lists active on-demand scans (<c>GET /shodan/scans</c>).</summary>
	Task<IReadOnlyList<ScanListEntry>> ListScansAsync(CancellationToken cancellationToken);

	/// <summary>Gets the status of a previously submitted scan (<c>GET /shodan/scans/{id}</c>).</summary>
	Task<ScanStatus> GetScanStatusAsync(string scanId, CancellationToken cancellationToken);
}
