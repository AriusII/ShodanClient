using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Dns.Wire;

/// <summary>Wire shape of the <c>GET /dns/domain/{domain}</c> response.</summary>
internal sealed class DomainDnsResponse
{
	public string? Domain { get; set; }

	public string[]? Tags { get; set; }

	public string[]? Subdomains { get; set; }

	public bool More { get; set; }

	public DnsRecordDto[]? Data { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>Wire shape of a single entry in the response's <c>data[]</c> array.</summary>
internal sealed class DnsRecordDto
{
	public string? Subdomain { get; set; }

	public string? Type { get; set; }

	public string? Value { get; set; }

	public string? LastSeen { get; set; }

	public int[]? Ports { get; set; }
}
