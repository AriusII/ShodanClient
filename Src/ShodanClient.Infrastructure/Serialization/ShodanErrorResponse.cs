using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Serialization;

/// <summary>
///     Wire shape of a Shodan error body. The core REST/Streaming APIs return <c>{ "error": "…" }</c>,
///     while InternetDB and some newer endpoints return FastAPI-style <c>{ "detail": "…" }</c>. This
///     DTO accepts either so <c>ShodanErrorTranslator</c> can surface a single message.
/// </summary>
internal sealed class ShodanErrorResponse
{
	/// <summary>The classic Shodan error message (<c>error</c>).</summary>
	[JsonPropertyName("error")]
	public string? Error { get; set; }

	/// <summary>The FastAPI-style error message (<c>detail</c>).</summary>
	[JsonPropertyName("detail")]
	public string? Detail { get; set; }

	/// <summary>The best available message, preferring <see cref="Error" /> then <see cref="Detail" />.</summary>
	[JsonIgnore]
	public string? Message => Error ?? Detail;
}
