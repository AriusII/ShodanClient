namespace ShodanClient.Domain.Search;

/// <summary>
///     The <c>http</c> module of a banner: details captured for an HTTP/HTTPS service.
/// </summary>
public sealed record HttpBanner
{
	/// <summary>HTTP status code of the probed response (<c>http.status</c>).</summary>
	public int? Status { get; init; }

	/// <summary>The <c>Host</c> header used for the probe (<c>http.host</c>).</summary>
	public string? Host { get; init; }

	/// <summary>The path that was requested (<c>http.location</c>).</summary>
	public string? Location { get; init; }

	/// <summary>Page title (<c>http.title</c>).</summary>
	public string? Title { get; init; }

	/// <summary>Server header (<c>http.server</c>).</summary>
	public string? Server { get; init; }

	/// <summary>Raw HTML body (<c>http.html</c>).</summary>
	public string? Html { get; init; }

	/// <summary>MurmurHash of the HTML body (<c>http.html_hash</c>).</summary>
	public long? HtmlHash { get; init; }

	/// <summary>Web Application Firewall detected, if any (<c>http.waf</c>).</summary>
	public string? Waf { get; init; }

	/// <summary>Response headers (<c>http.headers</c>).</summary>
	public IReadOnlyDictionary<string, string> Headers { get; init; } =
		new Dictionary<string, string>();

	/// <summary>Detected web technologies keyed by component name (<c>http.components</c>).</summary>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Components { get; init; } =
		new Dictionary<string, IReadOnlyList<string>>();

	/// <summary>The favicon captured for the service, if any (<c>http.favicon</c>).</summary>
	public HttpFavicon? Favicon { get; init; }

	/// <summary>Raw contents of <c>/robots.txt</c>, if crawled (<c>http.robots</c>).</summary>
	public string? Robots { get; init; }

	/// <summary>MurmurHash of <c>/robots.txt</c> (<c>http.robots_hash</c>).</summary>
	public long? RobotsHash { get; init; }

	/// <summary>Raw contents of <c>/sitemap.xml</c>, if crawled (<c>http.sitemap</c>).</summary>
	public string? Sitemap { get; init; }

	/// <summary>MurmurHash of <c>/sitemap.xml</c> (<c>http.sitemap_hash</c>).</summary>
	public long? SitemapHash { get; init; }

	/// <summary>Raw contents of <c>/.well-known/security.txt</c>, if crawled (<c>http.securitytxt</c>).</summary>
	public string? SecurityTxt { get; init; }

	/// <summary>MurmurHash of <c>/.well-known/security.txt</c> (<c>http.securitytxt_hash</c>).</summary>
	public long? SecurityTxtHash { get; init; }

	/// <summary>The chain of redirects followed to reach this banner, if any (<c>http.redirects</c>).</summary>
	public IReadOnlyList<HttpRedirect> Redirects { get; init; } = [];
}
