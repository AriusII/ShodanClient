using System.Collections.ObjectModel;
using ShodanClient.Domain.Search;
using ShodanClient.Domain.Trends;
using ShodanClient.Infrastructure.Trends.Wire;

namespace ShodanClient.Infrastructure.Trends.Mapping;

/// <summary>
///     Maps Trends wire DTOs onto pure domain models. Kept as static extension methods (rather than a
///     reflection-based mapper) so the whole graph stays allocation-light and Native-AOT/trim safe.
/// </summary>
internal static class TrendsMapper
{
	private static readonly IReadOnlyDictionary<string, IReadOnlyList<TrendFacetGroup>> EmptyFacets =
		ReadOnlyDictionary<string, IReadOnlyList<TrendFacetGroup>>.Empty;

	public static TrendResult ToDomain(this TrendSearchResponse dto) => new()
	{
		Total = dto.Total,
		Matches = dto.Matches is { Length: > 0 }
			? Array.ConvertAll(dto.Matches, static m => m.ToDomain())
			: [],
		Facets = ToFacetMap(dto.Facets)
	};

	private static TrendMatch ToDomain(this TrendMatchDto dto) => new(dto.Month ?? string.Empty, dto.Count);

	private static TrendFacetGroup ToDomain(this TrendFacetGroupDto dto) => new()
	{
		Key = dto.Key ?? string.Empty,
		Values = dto.Values is { Length: > 0 }
			? Array.ConvertAll(dto.Values, static v => new FacetItem(v.Value ?? string.Empty, v.Count))
			: []
	};

	private static IReadOnlyDictionary<string, IReadOnlyList<TrendFacetGroup>> ToFacetMap(
		Dictionary<string, TrendFacetGroupDto[]>? source)
	{
		if (source is null || source.Count == 0)
		{
			return EmptyFacets;
		}

		var result = new Dictionary<string, IReadOnlyList<TrendFacetGroup>>(source.Count, StringComparer.Ordinal);
		foreach (var (key, groups) in source)
		{
			result[key] = Array.ConvertAll(groups, static g => g.ToDomain());
		}

		return result;
	}
}
