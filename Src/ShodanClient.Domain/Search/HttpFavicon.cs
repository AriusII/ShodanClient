namespace ShodanClient.Domain.Search;

/// <summary>The favicon captured for an HTTP service (<c>http.favicon</c>).</summary>
public sealed record HttpFavicon
{
	/// <summary>Base64-encoded favicon bytes (<c>http.favicon.data</c>).</summary>
	public string? Data { get; init; }

	/// <summary>MurmurHash of the favicon (<c>http.favicon.hash</c>).</summary>
	public long? Hash { get; init; }

	/// <summary>The path the favicon was served from (<c>http.favicon.location</c>).</summary>
	public string? Location { get; init; }
}
