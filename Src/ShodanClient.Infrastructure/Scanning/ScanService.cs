using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Application.Common;
using ShodanClient.Application.Scanning;
using ShodanClient.Domain.Scanning;

namespace ShodanClient.Infrastructure.Scanning;

/// <summary>
///     Logic layer for requesting and monitoring on-demand scans. Validates inputs and delegates to
///     the repository.
/// </summary>
internal sealed class ScanService(IScanRepository repository) : IScanService
{
	public Task<IReadOnlyList<int>> GetCrawledPortsAsync(CancellationToken cancellationToken = default) =>
		repository.GetCrawledPortsAsync(cancellationToken);

	public Task<IReadOnlyDictionary<string, string>> GetProtocolsAsync(CancellationToken cancellationToken = default) =>
		repository.GetProtocolsAsync(cancellationToken);

	public Task<ScanSubmission> RequestScanAsync(
		IEnumerable<string> ipsOrNetblocks,
		CancellationToken cancellationToken = default)
	{
		var targets = Guard.NotNullOrEmpty(ipsOrNetblocks?.ToList());
		var csv = string.Join(",", targets);
		return repository.RequestScanAsync(new ScanRequestQuery(csv), cancellationToken);
	}

	public Task<ScanInternetResult> ScanInternetAsync(
		int port,
		string protocol,
		CancellationToken cancellationToken = default)
	{
		Guard.AtLeast(port, 1);
		Guard.NotNullOrWhiteSpace(protocol);
		return repository.ScanInternetAsync(new ScanInternetQuery(port, protocol), cancellationToken);
	}

	public Task<IReadOnlyList<ScanListEntry>> ListScansAsync(CancellationToken cancellationToken = default) =>
		repository.ListScansAsync(cancellationToken);

	public Task<ScanStatus> GetScanStatusAsync(string scanId, CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(scanId);
		return repository.GetScanStatusAsync(scanId, cancellationToken);
	}
}
