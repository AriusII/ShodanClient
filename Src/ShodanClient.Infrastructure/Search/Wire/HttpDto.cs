using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Search.Wire;

/// <summary>Wire shape of the <c>http</c> module.</summary>
internal sealed class HttpDto
{
	public int? Status { get; set; }

	public string? Host { get; set; }

	public string? Location { get; set; }

	public string? Title { get; set; }

	public string? Server { get; set; }

	public string? Html { get; set; }

	[JsonPropertyName("html_hash")] public long? HtmlHash { get; set; }

	public string? Waf { get; set; }

	public Dictionary<string, string>? Headers { get; set; }

	public Dictionary<string, HttpComponentDto>? Components { get; set; }

	public HttpFaviconDto? Favicon { get; set; }

	public string? Robots { get; set; }

	[JsonPropertyName("robots_hash")] public long? RobotsHash { get; set; }

	public string? Sitemap { get; set; }

	[JsonPropertyName("sitemap_hash")] public long? SitemapHash { get; set; }

	[JsonPropertyName("securitytxt")] public string? SecurityTxt { get; set; }

	[JsonPropertyName("securitytxt_hash")] public long? SecurityTxtHash { get; set; }

	public HttpRedirectDto[]? Redirects { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>Wire shape of a single entry in <c>http.redirects</c>.</summary>
internal sealed class HttpRedirectDto
{
	public string? Host { get; set; }

	public string? Location { get; set; }

	public string? Data { get; set; }
}

/// <summary>Wire shape of an <c>http.components</c> value.</summary>
internal sealed class HttpComponentDto
{
	public string[]? Categories { get; set; }
}

/// <summary>Wire shape of <c>http.favicon</c>.</summary>
internal sealed class HttpFaviconDto
{
	public string? Data { get; set; }

	public long? Hash { get; set; }

	public string? Location { get; set; }
}
