using System.Runtime.CompilerServices;
using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Domain.Search;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Search.Mapping;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Streaming;

/// <summary>Streaming implementation of <see cref="IStreamRepository" />.</summary>
internal sealed class StreamRepository(StreamingChannel channel) : IStreamRepository
{
	public async IAsyncEnumerable<Banner> StreamAllBannersAsync(
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Streaming.AllBanners();
		await foreach (var dto in channel
			               .StreamNdjsonAsync(route, ShodanJsonContext.Default.BannerDto, cancellationToken)
			               .ConfigureAwait(false))
		{
			yield return dto.ToDomain();
		}
	}

	public async IAsyncEnumerable<Banner> StreamByAsnAsync(
		IEnumerable<string> asns,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Streaming.Asn(asns);
		await foreach (var dto in channel
			               .StreamNdjsonAsync(route, ShodanJsonContext.Default.BannerDto, cancellationToken)
			               .ConfigureAwait(false))
		{
			yield return dto.ToDomain();
		}
	}

	public async IAsyncEnumerable<Banner> StreamByCountriesAsync(
		IEnumerable<string> countryCodes,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Streaming.Countries(countryCodes);
		await foreach (var dto in channel
			               .StreamNdjsonAsync(route, ShodanJsonContext.Default.BannerDto, cancellationToken)
			               .ConfigureAwait(false))
		{
			yield return dto.ToDomain();
		}
	}

	public async IAsyncEnumerable<Banner> StreamByPortsAsync(
		IEnumerable<int> ports,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Streaming.Ports(ports);
		await foreach (var dto in channel
			               .StreamNdjsonAsync(route, ShodanJsonContext.Default.BannerDto, cancellationToken)
			               .ConfigureAwait(false))
		{
			yield return dto.ToDomain();
		}
	}

	public async IAsyncEnumerable<Banner> StreamByVulnerabilitiesAsync(
		IEnumerable<string> cveIds,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Streaming.Vulnerabilities(cveIds);
		await foreach (var dto in channel
			               .StreamNdjsonAsync(route, ShodanJsonContext.Default.BannerDto, cancellationToken)
			               .ConfigureAwait(false))
		{
			yield return dto.ToDomain();
		}
	}

	public async IAsyncEnumerable<Banner> StreamByQueryAsync(
		string query,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Streaming.Custom(query);
		await foreach (var dto in channel
			               .StreamNdjsonAsync(route, ShodanJsonContext.Default.BannerDto, cancellationToken)
			               .ConfigureAwait(false))
		{
			yield return dto.ToDomain();
		}
	}

	public async IAsyncEnumerable<Banner> StreamAlertsAsync(
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Streaming.AllAlerts();
		await foreach (var dto in channel
			               .StreamNdjsonAsync(route, ShodanJsonContext.Default.BannerDto, cancellationToken)
			               .ConfigureAwait(false))
		{
			yield return dto.ToDomain();
		}
	}

	public async IAsyncEnumerable<Banner> StreamAlertAsync(
		string alertId,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Streaming.Alert(alertId);
		await foreach (var dto in channel
			               .StreamNdjsonAsync(route, ShodanJsonContext.Default.BannerDto, cancellationToken)
			               .ConfigureAwait(false))
		{
			yield return dto.ToDomain();
		}
	}
}
