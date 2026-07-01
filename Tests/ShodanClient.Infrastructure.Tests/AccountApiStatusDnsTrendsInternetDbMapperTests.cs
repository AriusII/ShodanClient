using ShodanClient.Infrastructure.Account.Mapping;
using ShodanClient.Infrastructure.Account.Wire;
using ShodanClient.Infrastructure.ApiStatus.Mapping;
using ShodanClient.Infrastructure.ApiStatus.Wire;
using ShodanClient.Infrastructure.Dns.Mapping;
using ShodanClient.Infrastructure.Dns.Wire;
using ShodanClient.Infrastructure.InternetDb.Mapping;
using ShodanClient.Infrastructure.InternetDb.Wire;
using ShodanClient.Infrastructure.Search.Wire;
using ShodanClient.Infrastructure.Trends.Mapping;
using ShodanClient.Infrastructure.Trends.Wire;

namespace ShodanClient.Infrastructure.Tests;

/// <summary>
///     Pure unit tests for the Account, ApiStatus, Dns, Trends and InternetDb Wire-to-Domain mappers:
///     field-by-field mapping of a fully populated DTO, and defaulting behavior for null/missing
///     optional fields (must yield empty collections/strings rather than nulls or exceptions).
/// </summary>
public sealed class AccountApiStatusDnsTrendsInternetDbMapperTests
{
	// ---------- AccountMapper ----------

	[Fact]
	public void AccountMapper_ToDomain_maps_all_fields_from_populated_dto()
	{
		var dto = new AccountProfileResponse
		{
			Member = true,
			Credits = 100,
			DisplayName = "AriusII",
			Created = "2020-09-29T09:39:45.813661"
		};

		var domain = dto.ToDomain();

		Assert.True(domain.Member);
		Assert.Equal(100, domain.Credits);
		Assert.Equal("AriusII", domain.DisplayName);
		Assert.NotNull(domain.Created);
		Assert.Equal(new DateTimeOffset(2020, 9, 29, 9, 39, 45, 813, TimeSpan.Zero).AddTicks(6610), domain.Created);
		Assert.Equal(TimeSpan.Zero, domain.Created!.Value.Offset);
	}

	[Fact]
	public void AccountMapper_ToDomain_null_displayName_and_created_map_to_null()
	{
		var dto = new AccountProfileResponse
		{
			Member = false,
			Credits = 0,
			DisplayName = null,
			Created = null
		};

		var domain = dto.ToDomain();

		Assert.Null(domain.DisplayName);
		Assert.Null(domain.Created);
	}

	// ---------- ApiStatusMapper ----------

	[Fact]
	public void ApiStatusMapper_ToDomain_maps_all_fields_including_nested_usage_limits()
	{
		var dto = new ApiInfoResponse
		{
			ScanCredits = 10,
			QueryCredits = 20,
			MonitoredIps = 5,
			Plan = "dev",
			Https = true,
			Telnet = false,
			Unlocked = true,
			UnlockedLeft = 3,
			UsageLimits = new UsageLimitsDto
			{
				ScanCredits = 100,
				QueryCredits = 200,
				MonitoredIps = 50
			}
		};

		var domain = dto.ToDomain();

		Assert.Equal(10, domain.ScanCredits);
		Assert.Equal(20, domain.QueryCredits);
		Assert.Equal(5, domain.MonitoredIps);
		Assert.Equal("dev", domain.Plan);
		Assert.True(domain.Https);
		Assert.False(domain.Telnet);
		Assert.True(domain.Unlocked);
		Assert.Equal(3, domain.UnlockedLeft);
		Assert.Equal(100, domain.UsageLimits.ScanCredits);
		Assert.Equal(200, domain.UsageLimits.QueryCredits);
		Assert.Equal(50, domain.UsageLimits.MonitoredIps);
	}

	[Fact]
	public void ApiStatusMapper_ToDomain_null_plan_defaults_to_empty_string()
	{
		var dto = new ApiInfoResponse
		{
			ScanCredits = 0,
			QueryCredits = 0,
			Plan = null,
			UsageLimits = new UsageLimitsDto { ScanCredits = 0, QueryCredits = 0 }
		};

		var domain = dto.ToDomain();

		Assert.Equal(string.Empty, domain.Plan);
	}

	[Fact]
	public void ApiStatusMapper_ToDomain_null_usage_limits_defaults_to_zero_credit_ceiling()
	{
		var dto = new ApiInfoResponse
		{
			ScanCredits = 1,
			QueryCredits = 2,
			Plan = "oss",
			UsageLimits = null
		};

		var domain = dto.ToDomain();

		Assert.Equal(0, domain.UsageLimits.ScanCredits);
		Assert.Equal(0, domain.UsageLimits.QueryCredits);
		Assert.Null(domain.UsageLimits.MonitoredIps);
	}

	// ---------- DnsMapper ----------

	[Fact]
	public void DnsMapper_ToDomain_maps_domain_tags_subdomains_and_records()
	{
		var dto = new DomainDnsResponse
		{
			Domain = "example.com",
			Tags = ["cdn"],
			Subdomains = ["www", "mail"],
			More = true,
			Data =
			[
				new DnsRecordDto
				{
					Subdomain = "www",
					Type = "A",
					Value = "93.184.216.34",
					LastSeen = "2024-01-15T10:20:30.123456",
					Ports = [80, 443]
				}
			]
		};

		var domain = dto.ToDomain();

		Assert.Equal("example.com", domain.Domain);
		Assert.Equal(["cdn"], domain.Tags);
		Assert.Equal(["www", "mail"], domain.Subdomains);
		Assert.True(domain.More);
		var record = Assert.Single(domain.Data);
		Assert.Equal("www", record.Subdomain);
		Assert.Equal("A", record.Type);
		Assert.Equal("93.184.216.34", record.Value);
		Assert.Equal([80, 443], record.Ports);
		Assert.NotNull(record.LastSeen);
		Assert.Equal(new DateOnly(2024, 1, 15), DateOnly.FromDateTime(record.LastSeen!.Value.UtcDateTime));
		Assert.Equal(TimeSpan.Zero, record.LastSeen!.Value.Offset);
	}

	[Fact]
	public void DnsMapper_ToDomain_null_tags_subdomains_and_data_default_to_empty_collections()
	{
		var dto = new DomainDnsResponse
		{
			Domain = null,
			Tags = null,
			Subdomains = null,
			Data = null
		};

		var domain = dto.ToDomain();

		Assert.Equal(string.Empty, domain.Domain);
		Assert.Empty(domain.Tags);
		Assert.Empty(domain.Subdomains);
		Assert.Empty(domain.Data);
	}

	[Fact]
	public void DnsMapper_ToDomain_record_with_null_fields_defaults_to_empty_strings_and_ports()
	{
		var dto = new DomainDnsResponse
		{
			Domain = "example.com",
			Data =
			[
				new DnsRecordDto
				{
					Subdomain = null,
					Type = null,
					Value = null,
					LastSeen = null,
					Ports = null
				}
			]
		};

		var record = Assert.Single(dto.ToDomain().Data);

		Assert.Equal(string.Empty, record.Subdomain);
		Assert.Equal(string.Empty, record.Type);
		Assert.Equal(string.Empty, record.Value);
		Assert.Null(record.LastSeen);
		Assert.Empty(record.Ports);
	}

	[Fact]
	public void DnsMapper_ToDomain_reverse_dns_dictionary_empty_returns_empty_readonly_dictionary()
	{
		var source = new Dictionary<string, string[]>();

		var domain = source.ToDomain();

		Assert.Empty(domain);
	}

	[Fact]
	public void DnsMapper_ToDomain_reverse_dns_dictionary_null_hostnames_become_empty_list()
	{
		var source = new Dictionary<string, string[]>
		{
			["8.8.8.8"] = ["dns.google"],
			["1.1.1.1"] = null!
		};

		var domain = source.ToDomain();

		Assert.Equal(["dns.google"], domain["8.8.8.8"]);
		Assert.Empty(domain["1.1.1.1"]);
	}

	// ---------- TrendsMapper ----------

	[Fact]
	public void TrendsMapper_ToDomain_maps_total_matches_and_facets()
	{
		var dto = new TrendSearchResponse
		{
			Total = 42,
			Matches =
			[
				new TrendMatchDto { Month = "2024-01", Count = 10 },
				new TrendMatchDto { Month = "2024-02", Count = 32 }
			],
			Facets = new Dictionary<string, TrendFacetGroupDto[]>
			{
				["country"] =
				[
					new TrendFacetGroupDto
					{
						Key = "2024-01",
						Values = [new FacetItemDto { Value = "US", Count = 5 }]
					}
				]
			}
		};

		var domain = dto.ToDomain();

		Assert.Equal(42, domain.Total);
		Assert.Equal(2, domain.Matches.Count);
		Assert.Equal("2024-01", domain.Matches[0].Month);
		Assert.Equal(10, domain.Matches[0].Count);
		var facetGroup = Assert.Single(domain.Facets["country"]);
		Assert.Equal("2024-01", facetGroup.Key);
		var facetValue = Assert.Single(facetGroup.Values);
		Assert.Equal("US", facetValue.Value);
		Assert.Equal(5, facetValue.Count);
	}

	[Fact]
	public void TrendsMapper_ToDomain_null_matches_and_facets_default_to_empty_collections()
	{
		var dto = new TrendSearchResponse
		{
			Total = 0,
			Matches = null,
			Facets = null
		};

		var domain = dto.ToDomain();

		Assert.Empty(domain.Matches);
		Assert.Empty(domain.Facets);
	}

	[Fact]
	public void TrendsMapper_ToDomain_null_month_and_facet_key_and_value_default_to_empty_strings()
	{
		var dto = new TrendSearchResponse
		{
			Total = 1,
			Matches = [new TrendMatchDto { Month = null, Count = 1 }],
			Facets = new Dictionary<string, TrendFacetGroupDto[]>
			{
				["asn"] =
				[
					new TrendFacetGroupDto { Key = null, Values = [new FacetItemDto { Value = null, Count = 1 }] }
				]
			}
		};

		var domain = dto.ToDomain();

		Assert.Equal(string.Empty, domain.Matches[0].Month);
		var facetGroup = Assert.Single(domain.Facets["asn"]);
		Assert.Equal(string.Empty, facetGroup.Key);
		Assert.Equal(string.Empty, facetGroup.Values[0].Value);
	}

	// ---------- InternetDbMapper ----------

	[Fact]
	public void InternetDbMapper_ToDomain_maps_all_fields_from_populated_dto()
	{
		var dto = new InternetDbResponse
		{
			Ip = "8.8.8.8",
			Ports = [53, 443],
			Cpes = ["cpe:/a:example:server"],
			Hostnames = ["dns.google"],
			Tags = ["cdn"],
			Vulns = ["CVE-2021-1234"]
		};

		var domain = dto.ToDomain();

		Assert.Equal("8.8.8.8", domain.Ip);
		Assert.Equal([53, 443], domain.Ports);
		Assert.Equal(["cpe:/a:example:server"], domain.Cpes);
		Assert.Equal(["dns.google"], domain.Hostnames);
		Assert.Equal(["cdn"], domain.Tags);
		Assert.Equal(["CVE-2021-1234"], domain.Vulnerabilities);
	}

	[Fact]
	public void InternetDbMapper_ToDomain_null_collections_default_to_empty_lists()
	{
		var dto = new InternetDbResponse
		{
			Ip = null,
			Ports = null,
			Cpes = null,
			Hostnames = null,
			Tags = null,
			Vulns = null
		};

		var domain = dto.ToDomain();

		Assert.Equal(string.Empty, domain.Ip);
		Assert.Empty(domain.Ports);
		Assert.Empty(domain.Cpes);
		Assert.Empty(domain.Hostnames);
		Assert.Empty(domain.Tags);
		Assert.Empty(domain.Vulnerabilities);
	}
}
