using ShodanClient.Domain.Directory;
using ShodanClient.Infrastructure.Directory.Wire;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Directory.Mapping;

/// <summary>
///     Maps Directory wire DTOs onto pure domain models. Kept as static extension methods (rather than
///     a reflection-based mapper) so the whole graph stays allocation-light and Native-AOT/trim safe.
/// </summary>
internal static class DirectoryMapper
{
	public static SavedQuery ToDomain(this SavedQueryDto dto) => new()
	{
		Query = dto.Query ?? string.Empty,
		Votes = dto.Votes,
		Title = dto.Title,
		Description = dto.Description,
		Timestamp = ShodanValueParsers.ParseTimestamp(dto.Timestamp),
		Tags = dto.Tags ?? []
	};

	public static SavedQueryResult ToDomain(this SavedQueryResponse dto) => new()
	{
		Matches = dto.Matches is { Length: > 0 }
			? Array.ConvertAll(dto.Matches, static q => q.ToDomain())
			: [],
		Total = dto.Total
	};

	public static QueryTag ToDomain(this QueryTagDto dto) => new(dto.Value ?? string.Empty, dto.Count);

	public static IReadOnlyList<QueryTag> ToDomain(this QueryTagsResponse dto) =>
		dto.Matches is { Length: > 0 }
			? Array.ConvertAll(dto.Matches, static t => t.ToDomain())
			: [];
}
