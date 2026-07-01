namespace ShodanClient.Domain.InternetDb;

/// <summary>
///     The fast, key-less summary for an IP from the InternetDB API: open ports, detected platforms,
///     hostnames, tags and known vulnerabilities. Updated weekly rather than in real time.
/// </summary>
public sealed record InternetDbHost
{
	/// <summary>The queried IP address (<c>ip</c>).</summary>
	public required string Ip { get; init; }

	/// <summary>Open ports discovered on the host (<c>ports</c>).</summary>
	public IReadOnlyList<int> Ports { get; init; } = [];

	/// <summary>CPE identifiers for the detected services (<c>cpes</c>).</summary>
	public IReadOnlyList<string> Cpes { get; init; } = [];

	/// <summary>Hostnames associated with the IP (<c>hostnames</c>).</summary>
	public IReadOnlyList<string> Hostnames { get; init; } = [];

	/// <summary>Classification tags (<c>tags</c>).</summary>
	public IReadOnlyList<string> Tags { get; init; } = [];

	/// <summary>CVE identifiers affecting the host (<c>vulns</c>).</summary>
	public IReadOnlyList<string> Vulnerabilities { get; init; } = [];
}
