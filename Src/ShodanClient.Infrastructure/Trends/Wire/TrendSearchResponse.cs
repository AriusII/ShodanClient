using System.Text.Json;
using System.Text.Json.Serialization;
using ShodanClient.Infrastructure.Search.Wire;

namespace ShodanClient.Infrastructure.Trends.Wire;

/// <summary>Wire shape of the <c>GET /api/v1/search</c> response on the Trends API.</summary>
internal sealed class TrendSearchResponse
{
	public long Total { get; set; }

	public TrendMatchDto[]? Matches { get; set; }

	public Dictionary<string, TrendFacetGroupDto[]>? Facets { get; set; }

	[JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

/// <summary>Wire shape of a single month's match count (<c>{ month, count }</c>).</summary>
internal sealed class TrendMatchDto
{
	public string? Month { get; set; }

	public long Count { get; set; }
}

/// <summary>Wire shape of a single month's facet breakdown (<c>{ key, values }</c>).</summary>
internal sealed class TrendFacetGroupDto
{
	public string? Key { get; set; }

	// Reuses the Search context's { count, value } bucket shape.
	public FacetItemDto[]? Values { get; set; }
}
