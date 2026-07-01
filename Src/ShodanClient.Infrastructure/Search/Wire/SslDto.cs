using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Search.Wire;

/// <summary>Wire shape of the <c>ssl</c> module.</summary>
internal sealed class SslDto
{
	public string[]? Versions { get; set; }

	public string[]? Alpn { get; set; }

	[JsonPropertyName("ja3s")] public string? Ja3S { get; set; }

	public string? Jarm { get; set; }

	public SslCipherDto? Cipher { get; set; }

	public SslCertDto? Cert { get; set; }

	public string[]? Chain { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>Wire shape of <c>ssl.cipher</c>.</summary>
internal sealed class SslCipherDto
{
	public string? Version { get; set; }

	public int? Bits { get; set; }

	public string? Name { get; set; }
}

/// <summary>Wire shape of <c>ssl.cert</c>.</summary>
internal sealed class SslCertDto
{
	public int? Version { get; set; }

	// Shodan may serialize the serial as a JSON number (possibly very large) or a string.
	public JsonElement? Serial { get; set; }

	public bool Expired { get; set; }

	public string? Issued { get; set; }

	public string? Expires { get; set; }

	[JsonPropertyName("sig_alg")] public string? SignatureAlgorithm { get; set; }

	public Dictionary<string, string>? Issuer { get; set; }

	public Dictionary<string, string>? Subject { get; set; }

	public Dictionary<string, string>? Fingerprint { get; set; }
}
