using System.Collections.ObjectModel;
using System.Text.Json;
using ShodanClient.Domain.Common;
using ShodanClient.Domain.Search;
using ShodanClient.Infrastructure.Search.Wire;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Search.Mapping;

/// <summary>
///     Maps Search wire DTOs onto pure domain models. Kept as static extension methods (rather than a
///     reflection-based mapper) so the whole graph stays allocation-light and Native-AOT/trim safe.
/// </summary>
internal static class ShodanSearchMapper
{
	private static readonly IReadOnlyDictionary<string, string> EmptyStringMap =
		ReadOnlyDictionary<string, string>.Empty;

	private static readonly IReadOnlyDictionary<string, Vulnerability> EmptyVulns =
		ReadOnlyDictionary<string, Vulnerability>.Empty;

	private static readonly IReadOnlyDictionary<string, IReadOnlyList<FacetItem>> EmptyFacets =
		ReadOnlyDictionary<string, IReadOnlyList<FacetItem>>.Empty;

	public static Host ToDomain(this HostResponse dto) => new()
	{
		IpString = dto.IpStr ?? string.Empty,
		Ip = dto.Ip,
		Ports = dto.Ports ?? [],
		Hostnames = dto.Hostnames ?? [],
		Domains = dto.Domains ?? [],
		Tags = dto.Tags ?? [],
		Vulnerabilities = dto.Vulns ?? [],
		Asn = dto.Asn,
		Isp = dto.Isp,
		Organization = dto.Org,
		OperatingSystem = dto.Os,
		LastUpdate = ShodanValueParsers.ParseTimestamp(dto.LastUpdate),
		Location = dto.ToGeoLocation(),
		Services = dto.Data is { Length: > 0 }
			? Array.ConvertAll(dto.Data, static b => b.ToDomain())
			: []
	};

	public static Banner ToDomain(this BannerDto dto) => new()
	{
		IpString = dto.IpStr ?? string.Empty,
		Ip = dto.Ip,
		IpV6 = dto.IpV6,
		Port = dto.Port,
		Transport = dto.Transport,
		Protocol = dto.Protocol,
		Data = dto.Data,
		Product = dto.Product,
		Version = dto.Version,
		Info = dto.Info,
		OperatingSystem = dto.Os,
		Cpe = ToCpeList(dto.Cpe),
		Cpe23 = ToCpeList(dto.Cpe23),
		Hostnames = dto.Hostnames ?? [],
		Domains = dto.Domains ?? [],
		Tags = dto.Tags ?? [],
		Asn = dto.Asn,
		Isp = dto.Isp,
		Organization = dto.Org,
		Timestamp = ShodanValueParsers.ParseTimestamp(dto.Timestamp),
		Hash = dto.Hash,
		Location = dto.Location?.ToDomain(),
		Metadata = dto.Shodan?.ToDomain(),
		Vulnerabilities = ToVulnMap(dto.Vulns),
		Http = dto.Http?.ToDomain(),
		Ssl = dto.Ssl?.ToDomain(),
		Dns = dto.Dns?.ToDomain()
	};

	public static SearchResult ToDomain(this SearchResponse dto) => new()
	{
		Matches = dto.Matches is { Length: > 0 }
			? Array.ConvertAll(dto.Matches, static b => b.ToDomain())
			: [],
		Total = dto.Total,
		Facets = ToFacetMap(dto.Facets)
	};

	public static CountResult ToDomain(this CountResponse dto) => new()
	{
		Total = dto.Total,
		Facets = ToFacetMap(dto.Facets)
	};

	public static QueryTokenParse ToDomain(this TokenParseResponse dto) => new()
	{
		SearchTerm = dto.SearchTerm,
		Attributes = ToStringMap(dto.Attributes),
		Filters = dto.Filters ?? [],
		Errors = dto.Errors ?? []
	};

	private static Vulnerability ToDomain(this VulnDto dto) => new()
	{
		Verified = dto.Verified,
		Cvss = dto.Cvss,
		CvssVersion = dto.CvssVersion,
		Epss = dto.Epss,
		RankingEpss = dto.RankingEpss,
		Kev = dto.Kev,
		Summary = dto.Summary,
		References = dto.References ?? []
	};

	private static GeoLocation ToDomain(this LocationDto dto) => new()
	{
		City = dto.City,
		RegionCode = dto.RegionCode,
		AreaCode = dto.AreaCode,
		PostalCode = dto.PostalCode,
		DmaCode = dto.DmaCode,
		CountryCode = dto.CountryCode,
		CountryCode3 = dto.CountryCode3,
		CountryName = dto.CountryName,
		Latitude = dto.Latitude,
		Longitude = dto.Longitude
	};

	private static GeoLocation ToGeoLocation(this HostResponse dto) => new()
	{
		City = dto.City,
		RegionCode = dto.RegionCode,
		AreaCode = dto.AreaCode,
		PostalCode = dto.PostalCode,
		DmaCode = dto.DmaCode,
		CountryCode = dto.CountryCode,
		CountryCode3 = dto.CountryCode3,
		CountryName = dto.CountryName,
		Latitude = dto.Latitude,
		Longitude = dto.Longitude
	};

	private static BannerShodanMetadata ToDomain(this ShodanMetaDto dto) => new()
	{
		Id = dto.Id,
		Module = dto.Module,
		Crawler = dto.Crawler,
		HostnamesFromReverseDns = dto.Ptr,
		Options = ToStringMap(dto.Options),
		Alert = dto.Alert is { } alert ? new BannerAlertReference { Id = alert.Id, Name = alert.Name } : null
	};

	private static HttpBanner ToDomain(this HttpDto dto) => new()
	{
		Status = dto.Status,
		Host = dto.Host,
		Location = dto.Location,
		Title = dto.Title,
		Server = dto.Server,
		Html = dto.Html,
		HtmlHash = dto.HtmlHash,
		Waf = dto.Waf,
		Headers = dto.Headers ?? EmptyStringMap,
		Components = ToComponentMap(dto.Components),
		Favicon = dto.Favicon?.ToDomain(),
		Robots = dto.Robots,
		RobotsHash = dto.RobotsHash,
		Sitemap = dto.Sitemap,
		SitemapHash = dto.SitemapHash,
		SecurityTxt = dto.SecurityTxt,
		SecurityTxtHash = dto.SecurityTxtHash,
		Redirects = dto.Redirects is { Length: > 0 }
			? Array.ConvertAll(dto.Redirects, static r => r.ToDomain())
			: []
	};

	private static HttpRedirect ToDomain(this HttpRedirectDto dto) => new()
	{
		Host = dto.Host,
		Location = dto.Location,
		Data = dto.Data
	};

	private static HttpFavicon ToDomain(this HttpFaviconDto dto) => new()
	{
		Data = dto.Data,
		Hash = dto.Hash,
		Location = dto.Location
	};

	private static SslBanner ToDomain(this SslDto dto) => new()
	{
		Versions = dto.Versions ?? [],
		Alpn = dto.Alpn ?? [],
		Ja3S = dto.Ja3S,
		Jarm = dto.Jarm,
		Cipher = dto.Cipher?.ToDomain(),
		Certificate = dto.Cert?.ToDomain(),
		Chain = dto.Chain ?? []
	};

	private static SslCipher ToDomain(this SslCipherDto dto) => new()
	{
		Version = dto.Version,
		Bits = dto.Bits,
		Name = dto.Name
	};

	private static BannerDnsModule ToDomain(this DnsModuleDto dto) => new()
	{
		ResolverHostname = dto.ResolverHostname,
		Recursive = dto.Recursive,
		ResolverId = dto.ResolverId,
		Software = dto.Software
	};

	private static SslCertificate ToDomain(this SslCertDto dto) => new()
	{
		Version = dto.Version,
		Serial = dto.Serial is { } serial ? AsString(serial) : null,
		Expired = dto.Expired,
		Issued = dto.Issued,
		Expires = dto.Expires,
		SignatureAlgorithm = dto.SignatureAlgorithm,
		Issuer = dto.Issuer ?? EmptyStringMap,
		Subject = dto.Subject ?? EmptyStringMap,
		Fingerprint = dto.Fingerprint ?? EmptyStringMap
	};

	private static IReadOnlyDictionary<string, Vulnerability> ToVulnMap(Dictionary<string, VulnDto>? source)
	{
		if (source is null || source.Count == 0)
		{
			return EmptyVulns;
		}

		var result = new Dictionary<string, Vulnerability>(source.Count, StringComparer.OrdinalIgnoreCase);
		foreach (var (key, value) in source)
		{
			result[key] = value.ToDomain();
		}

		return result;
	}

	private static IReadOnlyDictionary<string, IReadOnlyList<FacetItem>> ToFacetMap(
		Dictionary<string, FacetItemDto[]>? source)
	{
		if (source is null || source.Count == 0)
		{
			return EmptyFacets;
		}

		var result = new Dictionary<string, IReadOnlyList<FacetItem>>(source.Count, StringComparer.Ordinal);
		foreach (var (key, items) in source)
		{
			result[key] = Array.ConvertAll(items, static f => new FacetItem(f.Value ?? string.Empty, f.Count));
		}

		return result;
	}

	private static IReadOnlyDictionary<string, IReadOnlyList<string>> ToComponentMap(
		Dictionary<string, HttpComponentDto>? source)
	{
		if (source is null || source.Count == 0)
		{
			return ReadOnlyDictionary<string, IReadOnlyList<string>>.Empty;
		}

		var result = new Dictionary<string, IReadOnlyList<string>>(source.Count, StringComparer.Ordinal);
		foreach (var (key, value) in source)
		{
			result[key] = value.Categories ?? [];
		}

		return result;
	}

	private static IReadOnlyDictionary<string, string> ToStringMap(Dictionary<string, JsonElement>? source)
	{
		if (source is null || source.Count == 0)
		{
			return EmptyStringMap;
		}

		var result = new Dictionary<string, string>(source.Count, StringComparer.Ordinal);
		foreach (var (key, value) in source)
		{
			result[key] = AsString(value);
		}

		return result;
	}

	/// <summary>
	///     Parses each entry as a <see cref="Cpe" />, silently dropping any that fail to parse — the
	///     source is an external API response, not caller input, so a single malformed identifier
	///     should not fail the whole mapping.
	/// </summary>
	private static List<Cpe> ToCpeList(string[]? source)
	{
		if (source is not { Length: > 0 })
		{
			return [];
		}

		var result = new List<Cpe>(source.Length);
		foreach (var value in source)
		{
			if (Cpe.TryParse(value, out var cpe))
			{
				result.Add(cpe);
			}
		}

		return result;
	}

	private static string AsString(JsonElement element) =>
		element.ValueKind == JsonValueKind.String ? element.GetString() ?? string.Empty : element.GetRawText();
}
