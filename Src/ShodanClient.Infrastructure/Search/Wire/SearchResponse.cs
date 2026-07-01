using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Search.Wire;

/// <summary>Wire shape of the <c>GET /shodan/host/search</c> response.</summary>
internal sealed class SearchResponse
{
	public BannerDto[]? Matches { get; set; }

	public long Total { get; set; }

	public Dictionary<string, FacetItemDto[]>? Facets { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>Wire shape of the <c>GET /shodan/host/count</c> response.</summary>
internal sealed class CountResponse
{
	public long Total { get; set; }

	public Dictionary<string, FacetItemDto[]>? Facets { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>Wire shape of a single facet bucket (<c>{ count, value }</c>).</summary>
internal sealed class FacetItemDto
{
	public long Count { get; set; }

	[JsonPropertyName("value")] public string? Value { get; set; }
}
