using System.Text.Json;
using ShodanClient.Domain.Common;
using ShodanClient.Infrastructure.Search.Mapping;
using ShodanClient.Infrastructure.Search.Wire;

namespace ShodanClient.Infrastructure.Tests;

/// <summary>
///     Pure unit tests for <see cref="ShodanSearchMapper" />: wire DTO → domain mapping for the
///     <c>Host</c>/<c>Banner</c>/<c>SearchResult</c> graph, including the <c>http</c>/<c>ssl</c>
///     sub-modules and the <c>_shodan</c> collection metadata. No HTTP, no DI — DTOs are constructed
///     directly and mapped in-process.
/// </summary>
public sealed class SearchMapperTests
{
	[Fact]
	public void HostResponse_ToDomain_populated_maps_every_field_including_nested_banner()
	{
		var dto = new HostResponse
		{
			IpStr = "8.8.8.8",
			Ip = 134744072,
			Ports = [53, 443],
			Hostnames = ["dns.google"],
			Domains = ["google.com"],
			Tags = ["cdn"],
			Vulns = ["CVE-2020-0001"],
			Asn = "AS15169",
			Isp = "Google LLC",
			Org = "Google LLC",
			Os = "Linux",
			LastUpdate = "2024-01-15T12:00:00.500000",
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
			Data = [new BannerDto { IpStr = "8.8.8.8", Port = 53, Transport = "udp" }]
		};

		var host = dto.ToDomain();

		Assert.Equal("8.8.8.8", host.IpString);
		Assert.Equal(134744072, host.Ip);
		Assert.Equal([53, 443], host.Ports);
		Assert.Equal(["dns.google"], host.Hostnames);
		Assert.Equal(["google.com"], host.Domains);
		Assert.Equal(["cdn"], host.Tags);
		Assert.Equal(["CVE-2020-0001"], host.Vulnerabilities);
		Assert.Equal("AS15169", host.Asn);
		Assert.Equal("Google LLC", host.Isp);
		Assert.Equal("Google LLC", host.Organization);
		Assert.Equal("Linux", host.OperatingSystem);
		Assert.NotNull(host.LastUpdate);
		Assert.Equal(new DateTime(2024, 1, 15, 12, 0, 0, 500), host.LastUpdate!.Value.UtcDateTime);
		Assert.Equal(TimeSpan.Zero, host.LastUpdate.Value.Offset);
		Assert.NotNull(host.Location);
		Assert.Equal("Mountain View", host.Location!.City);
		Assert.Equal("CA", host.Location.RegionCode);
		Assert.Equal("94043", host.Location.PostalCode);
		Assert.Equal(650, host.Location.AreaCode);
		Assert.Equal(807, host.Location.DmaCode);
		Assert.Equal("US", host.Location.CountryCode);
		Assert.Equal("USA", host.Location.CountryCode3);
		Assert.Equal("United States", host.Location.CountryName);
		Assert.Equal(37.4056, host.Location.Latitude);
		Assert.Equal(-122.0775, host.Location.Longitude);
		var service = Assert.Single(host.Services);
		Assert.Equal(53, service.Port);
		Assert.Equal("udp", service.Transport);
	}

	[Fact]
	public void HostResponse_ToDomain_missing_optional_fields_default_to_empty_collections_not_null()
	{
		var dto = new HostResponse();

		var host = dto.ToDomain();

		Assert.Equal(string.Empty, host.IpString);
		Assert.Null(host.Ip);
		Assert.Empty(host.Ports);
		Assert.Empty(host.Hostnames);
		Assert.Empty(host.Domains);
		Assert.Empty(host.Tags);
		Assert.Empty(host.Vulnerabilities);
		Assert.Null(host.LastUpdate);
		Assert.NotNull(host.Location);
		Assert.Null(host.Location!.City);
		Assert.Empty(host.Services);
	}

	[Fact]
	public void BannerDto_ToDomain_populated_maps_nested_http_ssl_location_metadata_and_vulns()
	{
		var dto = new BannerDto
		{
			IpStr = "1.2.3.4",
			Ip = 16909060,
			IpV6 = "::1",
			Port = 443,
			Transport = "tcp",
			Protocol = "https",
			Data = "raw-banner-text",
			Product = "nginx",
			Version = "1.21.0",
			Info = "extra info",
			Os = "Linux",
			Cpe = ["cpe:/a:nginx:nginx"],
			Cpe23 = ["cpe:2.3:a:nginx:nginx:1.21.0"],
			Hostnames = ["example.com"],
			Domains = ["example.com"],
			Tags = ["cloud"],
			Asn = "AS15169",
			Isp = "Google",
			Org = "Google LLC",
			Timestamp = "2024-09-29T09:39:45.813661",
			Hash = 123456789L,
			Location = new LocationDto
			{
				City = "Mountain View",
				RegionCode = "CA",
				CountryCode = "US",
				CountryCode3 = "USA",
				CountryName = "United States",
				Latitude = 37.4056,
				Longitude = -122.0775
			},
			Shodan = new ShodanMetaDto
			{
				Id = "banner-id",
				Module = "https",
				Crawler = "crawler-1",
				Ptr = true,
				Options = new Dictionary<string, JsonElement> { ["hostname"] = ParseElement("\"example.com\"") },
				Alert = new ShodanMetaAlertDto { Id = "alert-1", Name = "My Alert" }
			},
			Vulns = new Dictionary<string, VulnDto>
			{
				["CVE-2021-1234"] = new()
				{
					Verified = true,
					Cvss = 7.5,
					CvssVersion = 3,
					Epss = 0.42,
					RankingEpss = 0.9,
					Kev = true,
					Summary = "desc",
					References = ["https://example.com/cve"]
				}
			},
			Http = new HttpDto { Status = 200, Title = "Example" },
			Ssl = new SslDto { Ja3S = "abc123" }
		};

		var banner = dto.ToDomain();

		Assert.Equal("1.2.3.4", banner.IpString);
		Assert.Equal(16909060, banner.Ip);
		Assert.Equal("::1", banner.IpV6);
		Assert.Equal(443, banner.Port);
		Assert.Equal("tcp", banner.Transport);
		Assert.Equal("https", banner.Protocol);
		Assert.Equal("raw-banner-text", banner.Data);
		Assert.Equal("nginx", banner.Product);
		Assert.Equal("1.21.0", banner.Version);
		Assert.Equal("extra info", banner.Info);
		Assert.Equal("Linux", banner.OperatingSystem);
		Assert.Equal([Cpe.Parse("cpe:/a:nginx:nginx")], banner.Cpe);
		Assert.Equal([Cpe.Parse("cpe:2.3:a:nginx:nginx:1.21.0")], banner.Cpe23);
		Assert.Equal(["example.com"], banner.Hostnames);
		Assert.Equal(["example.com"], banner.Domains);
		Assert.Equal(["cloud"], banner.Tags);
		Assert.Equal("AS15169", banner.Asn);
		Assert.Equal("Google", banner.Isp);
		Assert.Equal("Google LLC", banner.Organization);
		Assert.Equal(123456789L, banner.Hash);
		Assert.NotNull(banner.Timestamp);
		Assert.Equal(9, banner.Timestamp!.Value.Hour);
		Assert.Equal(39, banner.Timestamp.Value.Minute);
		Assert.Equal(45, banner.Timestamp.Value.Second);
		Assert.Equal(TimeSpan.Zero, banner.Timestamp.Value.Offset);

		Assert.NotNull(banner.Location);
		Assert.Equal("Mountain View", banner.Location!.City);
		Assert.Equal("US", banner.Location.CountryCode);

		Assert.NotNull(banner.Metadata);
		Assert.Equal("banner-id", banner.Metadata!.Id);
		Assert.Equal("https", banner.Metadata.Module);
		Assert.Equal("crawler-1", banner.Metadata.Crawler);
		Assert.True(banner.Metadata.HostnamesFromReverseDns);
		Assert.Equal("example.com", banner.Metadata.Options["hostname"]);
		Assert.NotNull(banner.Metadata.Alert);
		Assert.Equal("alert-1", banner.Metadata.Alert!.Id);
		Assert.Equal("My Alert", banner.Metadata.Alert.Name);

		Assert.Single(banner.Vulnerabilities);
		// The map is built with an OrdinalIgnoreCase comparer, so a differently-cased key must still resolve.
		var vuln = banner.Vulnerabilities["cve-2021-1234"];
		Assert.True(vuln.Verified);
		Assert.Equal(7.5, vuln.Cvss);
		Assert.Equal(3, vuln.CvssVersion);
		Assert.Equal(0.42, vuln.Epss);
		Assert.Equal(0.9, vuln.RankingEpss);
		Assert.True(vuln.Kev);
		Assert.Equal("desc", vuln.Summary);
		Assert.Equal(["https://example.com/cve"], vuln.References);

		Assert.Equal(200, banner.Http?.Status);
		Assert.Equal("Example", banner.Http?.Title);
		Assert.Equal("abc123", banner.Ssl?.Ja3S);
	}

	[Fact]
	public void BannerDto_ToDomain_null_nested_objects_map_to_null_without_throwing()
	{
		var dto = new BannerDto { IpStr = "1.1.1.1", Port = 80 };

		var banner = dto.ToDomain();

		Assert.Null(banner.Location);
		Assert.Null(banner.Metadata);
		Assert.Null(banner.Http);
		Assert.Null(banner.Ssl);
		Assert.Null(banner.Timestamp);
		Assert.Null(banner.Dns);
		Assert.Empty(banner.Vulnerabilities);
		Assert.Empty(banner.Cpe);
		Assert.Empty(banner.Cpe23);
		Assert.Empty(banner.Hostnames);
		Assert.Empty(banner.Domains);
		Assert.Empty(banner.Tags);
	}

	[Fact]
	public void BannerDto_ToDomain_shodanMeta_without_alert_maps_to_null_alert()
	{
		var dto = new BannerDto
		{
			IpStr = "1.1.1.1",
			Port = 80,
			Shodan = new ShodanMetaDto { Id = "x", Module = "http", Ptr = false, Alert = null }
		};

		var banner = dto.ToDomain();

		Assert.NotNull(banner.Metadata);
		Assert.Null(banner.Metadata!.Alert);
	}

	[Fact]
	public void BannerDto_ToDomain_unparseable_timestamp_maps_to_null()
	{
		var dto = new BannerDto { IpStr = "1.1.1.1", Port = 80, Timestamp = "not-a-real-timestamp" };

		var banner = dto.ToDomain();

		Assert.Null(banner.Timestamp);
	}

	[Fact]
	public void BannerDto_ToDomain_cpe_drops_null_and_whitespace_entries_but_keeps_valid_ones()
	{
		var dto = new BannerDto
		{
			IpStr = "1.1.1.1",
			Port = 80,
			Cpe = ["cpe:/a:nginx:nginx", "", "   ", null!],
			Cpe23 = ["cpe:2.3:a:nginx:nginx:1.21.0", "", "   ", null!]
		};

		var banner = dto.ToDomain();

		Assert.Equal([Cpe.Parse("cpe:/a:nginx:nginx")], banner.Cpe);
		Assert.Equal([Cpe.Parse("cpe:2.3:a:nginx:nginx:1.21.0")], banner.Cpe23);
	}

	[Fact]
	public void BannerDto_ToDomain_cpe_all_invalid_entries_map_to_empty_list_without_throwing()
	{
		var dto = new BannerDto { IpStr = "1.1.1.1", Port = 80, Cpe = ["", "   ", null!], Cpe23 = null };

		var banner = dto.ToDomain();

		Assert.Empty(banner.Cpe);
		Assert.Empty(banner.Cpe23);
	}

	[Fact]
	public void BannerDto_ToDomain_dns_populated_maps_all_fields()
	{
		var dto = new BannerDto
		{
			IpStr = "1.1.1.1",
			Port = 53,
			Dns = new DnsModuleDto
			{
				ResolverHostname = "resolver.example.com",
				Recursive = true,
				ResolverId = "abc123",
				Software = "BIND 9.16"
			}
		};

		var dns = dto.ToDomain().Dns;

		Assert.NotNull(dns);
		Assert.Equal("resolver.example.com", dns!.ResolverHostname);
		Assert.Equal(true, dns.Recursive);
		Assert.Equal("abc123", dns.ResolverId);
		Assert.Equal("BIND 9.16", dns.Software);
	}

	[Fact]
	public void HttpDto_ToDomain_maps_headers_components_favicon_and_redirects()
	{
		var dto = new BannerDto
		{
			IpStr = "1.1.1.1",
			Port = 443,
			Http = new HttpDto
			{
				Status = 200,
				Host = "example.com",
				Location = "/index.html",
				Title = "Example Domain",
				Server = "nginx/1.21.0",
				Html = "<html></html>",
				HtmlHash = 123,
				Waf = "Cloudflare",
				Headers = new Dictionary<string, string> { ["Content-Type"] = "text/html" },
				Components = new Dictionary<string, HttpComponentDto>
				{
					["jQuery"] = new() { Categories = ["JavaScript Libraries"] },
					["Unknown"] = new() { Categories = null }
				},
				Favicon = new HttpFaviconDto { Data = "base64data", Hash = 999, Location = "/favicon.ico" },
				Robots = "User-agent: *",
				RobotsHash = 1,
				Sitemap = "<xml/>",
				SitemapHash = 2,
				SecurityTxt = "Contact: security@example.com",
				SecurityTxtHash = 3,
				Redirects =
				[
					new HttpRedirectDto { Host = "old.example.com", Location = "https://example.com", Data = "raw" }
				]
			}
		};

		var http = dto.ToDomain().Http;

		Assert.NotNull(http);
		Assert.Equal(200, http!.Status);
		Assert.Equal("example.com", http.Host);
		Assert.Equal("/index.html", http.Location);
		Assert.Equal("Example Domain", http.Title);
		Assert.Equal("nginx/1.21.0", http.Server);
		Assert.Equal("<html></html>", http.Html);
		Assert.Equal(123, http.HtmlHash);
		Assert.Equal("Cloudflare", http.Waf);
		Assert.Equal("text/html", http.Headers["Content-Type"]);
		Assert.Equal(["JavaScript Libraries"], http.Components["jQuery"]);
		Assert.Empty(http.Components["Unknown"]);
		Assert.NotNull(http.Favicon);
		Assert.Equal("base64data", http.Favicon!.Data);
		Assert.Equal(999, http.Favicon.Hash);
		Assert.Equal("/favicon.ico", http.Favicon.Location);
		Assert.Equal("User-agent: *", http.Robots);
		Assert.Equal(1, http.RobotsHash);
		Assert.Equal("<xml/>", http.Sitemap);
		Assert.Equal(2, http.SitemapHash);
		Assert.Equal("Contact: security@example.com", http.SecurityTxt);
		Assert.Equal(3, http.SecurityTxtHash);
		var redirect = Assert.Single(http.Redirects);
		Assert.Equal("old.example.com", redirect.Host);
		Assert.Equal("https://example.com", redirect.Location);
		Assert.Equal("raw", redirect.Data);
	}

	[Fact]
	public void HttpDto_ToDomain_null_optional_fields_default_to_empty_collections_not_null()
	{
		var dto = new BannerDto { IpStr = "1.1.1.1", Port = 443, Http = new HttpDto { Status = 200 } };

		var http = dto.ToDomain().Http;

		Assert.NotNull(http);
		Assert.Empty(http!.Headers);
		Assert.Empty(http.Components);
		Assert.Null(http.Favicon);
		Assert.Empty(http.Redirects);
	}

	[Fact]
	public void SslDto_ToDomain_maps_cipher_and_certificate_fields()
	{
		var dto = new BannerDto
		{
			IpStr = "1.1.1.1",
			Port = 443,
			Ssl = new SslDto
			{
				Versions = ["TLSv1.2", "-TLSv1.0"],
				Alpn = ["h2", "http/1.1"],
				Ja3S = "abc123",
				Jarm = "def456",
				Cipher = new SslCipherDto { Version = "TLSv1.2", Bits = 128, Name = "ECDHE-RSA-AES128-GCM-SHA256" },
				Cert = new SslCertDto
				{
					Version = 2,
					Serial = ParseElement("\"0A1B2C3D\""),
					Expired = false,
					Issued = "20240101000000Z",
					Expires = "20250101000000Z",
					SignatureAlgorithm = "sha256WithRSAEncryption",
					Issuer = new Dictionary<string, string> { ["CN"] = "Let's Encrypt" },
					Subject = new Dictionary<string, string> { ["CN"] = "example.com" },
					Fingerprint = new Dictionary<string, string> { ["sha256"] = "AA:BB:CC" }
				},
				Chain = ["-----BEGIN CERTIFICATE-----..."]
			}
		};

		var ssl = dto.ToDomain().Ssl;

		Assert.NotNull(ssl);
		Assert.Equal(["TLSv1.2", "-TLSv1.0"], ssl!.Versions);
		Assert.Equal(["h2", "http/1.1"], ssl.Alpn);
		Assert.Equal("abc123", ssl.Ja3S);
		Assert.Equal("def456", ssl.Jarm);
		Assert.NotNull(ssl.Cipher);
		Assert.Equal("TLSv1.2", ssl.Cipher!.Version);
		Assert.Equal(128, ssl.Cipher.Bits);
		Assert.Equal("ECDHE-RSA-AES128-GCM-SHA256", ssl.Cipher.Name);
		Assert.NotNull(ssl.Certificate);
		Assert.Equal(2, ssl.Certificate!.Version);
		Assert.Equal("0A1B2C3D", ssl.Certificate.Serial);
		Assert.False(ssl.Certificate.Expired);
		Assert.Equal("20240101000000Z", ssl.Certificate.Issued);
		Assert.Equal("20250101000000Z", ssl.Certificate.Expires);
		Assert.Equal("sha256WithRSAEncryption", ssl.Certificate.SignatureAlgorithm);
		Assert.Equal("Let's Encrypt", ssl.Certificate.Issuer["CN"]);
		Assert.Equal("example.com", ssl.Certificate.Subject["CN"]);
		Assert.Equal("AA:BB:CC", ssl.Certificate.Fingerprint["sha256"]);
		Assert.Equal(["-----BEGIN CERTIFICATE-----..."], ssl.Chain);
	}

	[Fact]
	public void SslCertDto_ToDomain_serial_as_json_number_maps_to_raw_numeric_text()
	{
		// Shodan may serialize a certificate serial as a bare JSON number (possibly larger than any
		// integer type can hold), in which case the mapper falls back to the element's raw text.
		var dto = new BannerDto
		{
			IpStr = "1.1.1.1",
			Port = 443,
			Ssl = new SslDto { Cert = new SslCertDto { Serial = ParseElement("123456789012345678901234567890") } }
		};

		var serial = dto.ToDomain().Ssl?.Certificate?.Serial;

		Assert.Equal("123456789012345678901234567890", serial);
	}

	[Fact]
	public void SslCertDto_ToDomain_null_serial_maps_to_null()
	{
		var dto = new BannerDto
		{
			IpStr = "1.1.1.1",
			Port = 443,
			Ssl = new SslDto { Cert = new SslCertDto { Serial = null } }
		};

		var serial = dto.ToDomain().Ssl?.Certificate?.Serial;

		Assert.Null(serial);
	}

	[Fact]
	public void SearchResponse_ToDomain_maps_matches_total_and_facets()
	{
		var dto = new SearchResponse
		{
			Matches = [new BannerDto { IpStr = "1.1.1.1", Port = 80, Transport = "tcp" }],
			Total = 42,
			Facets = new Dictionary<string, FacetItemDto[]>
			{
				["country"] =
				[
					new FacetItemDto { Count = 5, Value = "US" },
					new FacetItemDto { Count = 2, Value = null }
				]
			}
		};

		var result = dto.ToDomain();

		Assert.Equal(42, result.Total);
		var match = Assert.Single(result.Matches);
		Assert.Equal(80, match.Port);
		Assert.Equal("tcp", match.Transport);
		var countFacet = result.Facets["country"];
		Assert.Equal("US", countFacet[0].Value);
		Assert.Equal(5, countFacet[0].Count);
		// A missing facet value defaults to an empty string rather than propagating null.
		Assert.Equal(string.Empty, countFacet[1].Value);
		Assert.Equal(2, countFacet[1].Count);
	}

	[Fact]
	public void SearchResponse_ToDomain_null_matches_and_facets_default_to_empty_not_null()
	{
		var dto = new SearchResponse { Matches = null, Total = 0, Facets = null };

		var result = dto.ToDomain();

		Assert.Empty(result.Matches);
		Assert.Empty(result.Facets);
		Assert.Equal(0, result.Total);
	}

	private static JsonElement ParseElement(string json) => JsonDocument.Parse(json).RootElement;
}
