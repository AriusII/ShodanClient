using System.Text.Json;
using BenchmarkDotNet.Attributes;
using ShodanClient.Domain.Alerts;
using ShodanClient.Domain.Search;
using ShodanClient.Infrastructure.Alerts.Mapping;
using ShodanClient.Infrastructure.Alerts.Wire;
using ShodanClient.Infrastructure.Search.Mapping;
using ShodanClient.Infrastructure.Search.Wire;

namespace ShodanClient.Benchmarks;

/// <summary>
///     Establishes a baseline allocation profile for the internal DTO-to-domain mapping layer
///     (<see cref="ShodanSearchMapper" /> and <see cref="AlertMapper" />) so future regressions in mapping
///     cost or allocations become visible. These two benchmarks are absolute cost measurements for
///     realistically-populated wire shapes, not a comparison between alternative approaches.
/// </summary>
[MemoryDiagnoser]
public class MapperBenchmarks
{
	private static readonly HostResponse HostFixture = BuildHostResponse();
	private static readonly AlertDto[] AlertsFixture = BuildAlerts();

	[Benchmark(Description = "ShodanSearchMapper: HostResponse (5 banners) -> Host")]
	public Host HostToDomain() => HostFixture.ToDomain();

	[Benchmark(Description = "AlertMapper: AlertDto[10] -> IReadOnlyList<Alert>")]
	public IReadOnlyList<Alert> AlertsToDomain() => AlertsFixture.ToDomain();

	private static HostResponse BuildHostResponse()
	{
		var banners = new BannerDto[5];
		for (var i = 0; i < banners.Length; i++)
		{
			banners[i] = BuildBanner(i);
		}

		return new HostResponse
		{
			IpStr = "8.8.8.8",
			Ip = 134744072,
			Ports = [22, 80, 443, 8080, 3306],
			Hostnames = ["dns.google"],
			Domains = ["google"],
			Tags = ["cloud"],
			Vulns = ["CVE-2021-1234", "CVE-2022-5678"],
			Asn = "AS15169",
			Isp = "Google LLC",
			Org = "Google LLC",
			Os = "Ubuntu",
			LastUpdate = "2024-09-29T09:39:45.813661",
			City = "Mountain View",
			RegionCode = "CA",
			PostalCode = "94043",
			AreaCode = 650,
			DmaCode = 807,
			CountryCode = "US",
			CountryCode3 = "USA",
			CountryName = "United States",
			Latitude = 37.4056,
			Longitude = -122.0775,
			Data = banners
		};
	}

	private static BannerDto BuildBanner(int index)
	{
		var port = index switch
		{
			0 => 22,
			1 => 80,
			2 => 443,
			3 => 8080,
			_ => 3306
		};

		return new BannerDto
		{
			IpStr = "8.8.8.8",
			Ip = 134744072,
			IpV6 = null,
			Port = port,
			Transport = "tcp",
			Protocol = "tcp",
			Data = $"Banner data for port {port}\r\n\r\n",
			Product = "nginx",
			Version = "1.25.3",
			Info = "Ubuntu",
			Os = "Ubuntu",
			Cpe = ["cpe:/a:nginx:nginx:1.25.3"],
			Cpe23 = ["cpe:2.3:a:nginx:nginx:1.25.3:*:*:*:*:*:*:*"],
			Hostnames = ["dns.google"],
			Domains = ["google"],
			Tags = ["cloud"],
			Asn = "AS15169",
			Isp = "Google LLC",
			Org = "Google LLC",
			Timestamp = "2024-09-29T09:39:45.813661",
			Hash = 123456789 + index,
			Location = new LocationDto
			{
				City = "Mountain View",
				RegionCode = "CA",
				AreaCode = 650,
				PostalCode = "94043",
				DmaCode = 807,
				CountryCode = "US",
				CountryCode3 = "USA",
				CountryName = "United States",
				Latitude = 37.4056,
				Longitude = -122.0775
			},
			Shodan = new ShodanMetaDto
			{
				Id = $"abc-{index}",
				Module = "https",
				Crawler = "crawler-1",
				Ptr = false,
				Options = new Dictionary<string, JsonElement>
				{
					["referrer"] = JsonDocument.Parse("\"dns.google\"").RootElement
				}
			},
			Vulns = new Dictionary<string, VulnDto>
			{
				["CVE-2021-1234"] = new()
				{
					Verified = true,
					Cvss = 7.5,
					CvssVersion = 3,
					Epss = 0.02,
					RankingEpss = 0.5,
					Kev = false,
					Summary = "Example vulnerability summary used for benchmarking purposes.",
					References = ["https://nvd.nist.gov/vuln/detail/CVE-2021-1234"]
				}
			},
			Http = new HttpDto
			{
				Status = 200,
				Host = "dns.google",
				Location = "/",
				Title = "Example",
				Server = "nginx",
				Html = "<html><body>Example</body></html>",
				HtmlHash = 987654321,
				Waf = null,
				Headers = new Dictionary<string, string>
				{
					["Content-Type"] = "text/html",
					["Server"] = "nginx"
				},
				Components = new Dictionary<string, HttpComponentDto>
				{
					["Nginx"] = new() { Categories = ["web-servers"] }
				},
				Favicon = new HttpFaviconDto
				{
					Data = "AAABAAEAEBAAAAEAIABoBAAAFgAAACgAAAAQ",
					Hash = 111222333,
					Location = "/favicon.ico"
				},
				Robots = "User-agent: *\nDisallow:",
				RobotsHash = 444555666,
				Sitemap = "<urlset></urlset>",
				SitemapHash = 777888999,
				SecurityTxt = null,
				SecurityTxtHash = null,
				Redirects =
				[
					new HttpRedirectDto
					{
						Host = "dns.google", Location = "https://dns.google/", Data = "HTTP/1.1 301 Moved"
					}
				]
			},
			Ssl = new SslDto
			{
				Versions = ["TLSv1.2", "TLSv1.3"],
				Alpn = ["h2", "http/1.1"],
				Ja3S = "a0e9f5d64349fb13191bc781f81f42e1",
				Jarm = "27d40d40d29d40d1dc42d43d00041d4689e4e12a97ac8ecf3771111176c53",
				Cipher = new SslCipherDto { Version = "TLSv1.3", Bits = 256, Name = "TLS_AES_256_GCM_SHA384" },
				Cert = new SslCertDto
				{
					Version = 2,
					Expired = false,
					Issued = "20240101000000Z",
					Expires = "20250101000000Z",
					SignatureAlgorithm = "sha256WithRSAEncryption",
					Issuer = new Dictionary<string, string>
						{ ["C"] = "US", ["O"] = "Example CA", ["CN"] = "Example CA" },
					Subject = new Dictionary<string, string> { ["CN"] = "dns.google" },
					Fingerprint = new Dictionary<string, string> { ["sha256"] = "ab:cd:ef:01:23:45:67:89" }
				},
				Chain = ["-----BEGIN CERTIFICATE-----...", "-----BEGIN CERTIFICATE-----..."]
			}
		};
	}

	private static AlertDto[] BuildAlerts()
	{
		var alerts = new AlertDto[10];
		for (var i = 0; i < alerts.Length; i++)
		{
			alerts[i] = new AlertDto
			{
				Id = $"alert-{i}",
				Name = $"Monitor {i}",
				Created = "2024-01-01T00:00:00.000000",
				Expires = 86400,
				Size = 100,
				Filters = new AlertFiltersDto { Ip = ["8.8.8.8", "1.1.1.1"] },
				Triggers = new Dictionary<string, AlertTriggerDto>
				{
					["malware"] = new() { Rule = "malware" },
					["open_database"] = new() { Rule = "open_database" },
					["vulnerable"] = new() { Rule = "vulnerable" },
					["ssl_expired"] = new() { Rule = "ssl_expired" }
				},
				HasTriggers = true,
				Expiration = "2024-12-31T23:59:59.000000",
				Notifiers =
				[
					new AlertNotifierRefDto { Id = $"notifier-{i}-a", Provider = "email" },
					new AlertNotifierRefDto { Id = $"notifier-{i}-b", Provider = "slack" }
				]
			};
		}

		return alerts;
	}
}
