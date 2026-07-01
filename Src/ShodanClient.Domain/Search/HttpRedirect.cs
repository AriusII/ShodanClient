namespace ShodanClient.Domain.Search;

/// <summary>A single hop recorded while following HTTP redirects (<c>http.redirects[]</c>).</summary>
public sealed record HttpRedirect
{
	/// <summary>The host that issued the redirect (<c>host</c>).</summary>
	public string? Host { get; init; }

	/// <summary>The redirect target (<c>location</c>).</summary>
	public string? Location { get; init; }

	/// <summary>The raw response data captured for this hop, if any (<c>data</c>).</summary>
	public string? Data { get; init; }
}
