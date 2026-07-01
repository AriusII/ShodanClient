namespace ShodanClient.Domain.Search;

/// <summary>
///     The <c>ssl</c> module of a banner: TLS details captured for an encrypted service.
/// </summary>
public sealed record SslBanner
{
	/// <summary>Supported/unsupported TLS versions (<c>ssl.versions</c>; a leading <c>-</c> means unsupported).</summary>
	public IReadOnlyList<string> Versions { get; init; } = [];

	/// <summary>ALPN protocols advertised (<c>ssl.alpn</c>).</summary>
	public IReadOnlyList<string> Alpn { get; init; } = [];

	/// <summary>JA3S server fingerprint (<c>ssl.ja3s</c>).</summary>
	public string? Ja3S { get; init; }

	/// <summary>JARM fingerprint (<c>ssl.jarm</c>).</summary>
	public string? Jarm { get; init; }

	/// <summary>The negotiated cipher (<c>ssl.cipher</c>).</summary>
	public SslCipher? Cipher { get; init; }

	/// <summary>The presented certificate (<c>ssl.cert</c>).</summary>
	public SslCertificate? Certificate { get; init; }

	/// <summary>PEM-encoded certificate chain (<c>ssl.chain</c>).</summary>
	public IReadOnlyList<string> Chain { get; init; } = [];
}
