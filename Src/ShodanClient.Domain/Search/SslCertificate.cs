namespace ShodanClient.Domain.Search;

/// <summary>An X.509 certificate presented by a TLS service (<c>ssl.cert</c>).</summary>
public sealed record SslCertificate
{
	/// <summary>Certificate version (<c>ssl.cert.version</c>).</summary>
	public int? Version { get; init; }

	/// <summary>Serial number as reported by Shodan (<c>ssl.cert.serial</c>).</summary>
	public string? Serial { get; init; }

	/// <summary>Whether the certificate has expired (<c>ssl.cert.expired</c>).</summary>
	public bool Expired { get; init; }

	/// <summary>Not-before timestamp string (<c>ssl.cert.issued</c>).</summary>
	public string? Issued { get; init; }

	/// <summary>Not-after timestamp string (<c>ssl.cert.expires</c>).</summary>
	public string? Expires { get; init; }

	/// <summary>Signature algorithm (<c>ssl.cert.sig_alg</c>).</summary>
	public string? SignatureAlgorithm { get; init; }

	/// <summary>Issuer distinguished-name components (<c>ssl.cert.issuer</c>).</summary>
	public IReadOnlyDictionary<string, string> Issuer { get; init; } =
		new Dictionary<string, string>();

	/// <summary>Subject distinguished-name components (<c>ssl.cert.subject</c>).</summary>
	public IReadOnlyDictionary<string, string> Subject { get; init; } =
		new Dictionary<string, string>();

	/// <summary>Certificate fingerprints keyed by algorithm (<c>ssl.cert.fingerprint</c>).</summary>
	public IReadOnlyDictionary<string, string> Fingerprint { get; init; } =
		new Dictionary<string, string>();
}
