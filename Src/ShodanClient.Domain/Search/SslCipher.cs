namespace ShodanClient.Domain.Search;

/// <summary>The negotiated cipher for a TLS service (<c>ssl.cipher</c>).</summary>
public sealed record SslCipher
{
	/// <summary>TLS version the cipher applies to (<c>ssl.cipher.version</c>).</summary>
	public string? Version { get; init; }

	/// <summary>Cipher strength in bits (<c>ssl.cipher.bits</c>).</summary>
	public int? Bits { get; init; }

	/// <summary>Cipher name (<c>ssl.cipher.name</c>).</summary>
	public string? Name { get; init; }
}
