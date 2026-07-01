using System.Globalization;
using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Scanning;
using ShodanClient.Domain.Scanning;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Scanning.Mapping;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Scanning;

/// <summary>REST implementation of <see cref="IScanRepository" />.</summary>
internal sealed class ScanRepository(RestChannel channel) : IScanRepository
{
	public async Task<IReadOnlyList<int>> GetCrawledPortsAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Scanning.Ports();
		return await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.Int32Array, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task<IReadOnlyDictionary<string, string>> GetProtocolsAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Scanning.Protocols();
		return await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.DictionaryStringString, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task<ScanSubmission> RequestScanAsync(ScanRequestQuery query, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Scanning.Scan();
		var form = new[] { new KeyValuePair<string, string>("ips", query.Ips) };
		var response = await channel
			.SendFormAsync(route, form, ShodanJsonContext.Default.ScanSubmissionResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<ScanInternetResult> ScanInternetAsync(ScanInternetQuery query,
		CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Scanning.ScanInternet();
		var form = new[]
		{
			new KeyValuePair<string, string>("port", query.Port.ToString(CultureInfo.InvariantCulture)),
			new KeyValuePair<string, string>("protocol", query.Protocol)
		};
		var response = await channel
			.SendFormAsync(route, form, ShodanJsonContext.Default.ScanInternetResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<IReadOnlyList<ScanListEntry>> ListScansAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Scanning.Scans();
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.ScanListResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<ScanStatus> GetScanStatusAsync(string scanId, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Scanning.ScanStatus(scanId);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.ScanStatusResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}
}
