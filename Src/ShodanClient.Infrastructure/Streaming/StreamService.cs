using System.Runtime.CompilerServices;
using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Application.Common;
using ShodanClient.Domain.Search;

namespace ShodanClient.Infrastructure.Streaming;

/// <summary>
///     Logic layer for consuming Shodan's streaming banner feeds. Validates inputs and delegates the
///     long-lived enumeration to the repository.
/// </summary>
internal sealed class StreamService(IStreamRepository repository) : IStreamService
{
	public async IAsyncEnumerable<Banner> StreamAllBannersAsync(
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var banner in repository.StreamAllBannersAsync(cancellationToken).ConfigureAwait(false))
		{
			yield return banner;
		}
	}

	public async IAsyncEnumerable<Banner> StreamByAsnAsync(
		IEnumerable<string> asns,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var targets = Guard.NotNullOrEmpty(asns as IReadOnlyList<string> ?? asns?.ToList());
		await foreach (var banner in repository.StreamByAsnAsync(targets, cancellationToken).ConfigureAwait(false))
		{
			yield return banner;
		}
	}

	public async IAsyncEnumerable<Banner> StreamByCountriesAsync(
		IEnumerable<string> countryCodes,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var targets = Guard.NotNullOrEmpty(countryCodes as IReadOnlyList<string> ?? countryCodes?.ToList());
		await foreach (var banner in repository.StreamByCountriesAsync(targets, cancellationToken)
			               .ConfigureAwait(false))
		{
			yield return banner;
		}
	}

	public async IAsyncEnumerable<Banner> StreamByPortsAsync(
		IEnumerable<int> ports,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var targets = Guard.NotNullOrEmpty(ports as IReadOnlyList<int> ?? ports?.ToList());
		await foreach (var banner in repository.StreamByPortsAsync(targets, cancellationToken).ConfigureAwait(false))
		{
			yield return banner;
		}
	}

	public async IAsyncEnumerable<Banner> StreamByVulnerabilitiesAsync(
		IEnumerable<string> cveIds,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var targets = Guard.NotNullOrEmpty(cveIds as IReadOnlyList<string> ?? cveIds?.ToList());
		await foreach (var banner in repository.StreamByVulnerabilitiesAsync(targets, cancellationToken)
			               .ConfigureAwait(false))
		{
			yield return banner;
		}
	}

	public async IAsyncEnumerable<Banner> StreamByQueryAsync(
		string query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(query);
		await foreach (var banner in repository.StreamByQueryAsync(query, cancellationToken).ConfigureAwait(false))
		{
			yield return banner;
		}
	}

	public async IAsyncEnumerable<Banner> StreamAlertsAsync(
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var banner in repository.StreamAlertsAsync(cancellationToken).ConfigureAwait(false))
		{
			yield return banner;
		}
	}

	public async IAsyncEnumerable<Banner> StreamAlertAsync(
		string alertId,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(alertId);
		await foreach (var banner in repository.StreamAlertAsync(alertId, cancellationToken).ConfigureAwait(false))
		{
			yield return banner;
		}
	}
}
