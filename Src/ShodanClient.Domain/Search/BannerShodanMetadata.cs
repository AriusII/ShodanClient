namespace ShodanClient.Domain.Search;

/// <summary>
///     The <c>_shodan</c> block describing how a banner was collected.
/// </summary>
public sealed record BannerShodanMetadata
{
	/// <summary>Unique identifier of the banner (<c>_shodan.id</c>).</summary>
	public string? Id { get; init; }

	/// <summary>The crawler module that produced the banner (<c>_shodan.module</c>, e.g. <c>https</c>).</summary>
	public string? Module { get; init; }

	/// <summary>Identifier of the crawler node (<c>_shodan.crawler</c>).</summary>
	public string? Crawler { get; init; }

	/// <summary>Whether the hostnames were derived from a reverse-DNS (PTR) lookup (<c>_shodan.ptr</c>).</summary>
	public bool HostnamesFromReverseDns { get; init; }

	/// <summary>Additional collection options such as the requested hostname (<c>_shodan.options</c>).</summary>
	public IReadOnlyDictionary<string, string> Options { get; init; } =
		new Dictionary<string, string>();

	/// <summary>
	///     The network alert that produced this banner, present only on the alert-stream endpoints
	///     (<c>_shodan.alert</c>).
	/// </summary>
	public BannerAlertReference? Alert { get; init; }
}
