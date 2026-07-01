using System.Collections.ObjectModel;
using ShodanClient.Domain.Dns;
using ShodanClient.Infrastructure.Dns.Wire;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Dns.Mapping;

/// <summary>
///     Maps DNS wire DTOs onto pure domain models. Kept as static extension methods (rather than a
///     reflection-based mapper) so the whole graph stays allocation-light and Native-AOT/trim safe.
/// </summary>
internal static class DnsMapper
{
	public static DomainDnsInfo ToDomain(this DomainDnsResponse dto) => new()
	{
		Domain = dto.Domain ?? string.Empty,
		Tags = dto.Tags ?? [],
		Subdomains = dto.Subdomains ?? [],
		More = dto.More,
		Data = dto.Data is { Length: > 0 }
			? Array.ConvertAll(dto.Data, static d => d.ToDomain())
			: []
	};

	private static DnsRecord ToDomain(this DnsRecordDto dto) => new()
	{
		Subdomain = dto.Subdomain ?? string.Empty,
		Type = dto.Type ?? string.Empty,
		Value = dto.Value ?? string.Empty,
		LastSeen = ShodanValueParsers.ParseTimestamp(dto.LastSeen),
		Ports = dto.Ports ?? []
	};

	/// <summary>
	///     Converts a raw <c>GET /dns/reverse</c> map (<c>ip_str -&gt; hostnames[] | null</c>) into a
	///     domain-friendly map where an IP with no PTR record is an empty list rather than
	///     <see langword="null" />.
	/// </summary>
	public static IReadOnlyDictionary<string, IReadOnlyList<string>> ToDomain(this Dictionary<string, string[]> source)
	{
		if (source.Count == 0)
		{
			return ReadOnlyDictionary<string, IReadOnlyList<string>>.Empty;
		}

		var result = new Dictionary<string, IReadOnlyList<string>>(source.Count, StringComparer.Ordinal);
		foreach (var (key, value) in source)
		{
			result[key] = value ?? [];
		}

		return result;
	}
}
