using ShodanClient.Domain.BulkData;
using ShodanClient.Infrastructure.BulkData.Wire;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.BulkData.Mapping;

/// <summary>
///     Maps BulkData wire DTOs onto pure domain models. Kept as static extension methods (rather than
///     a reflection-based mapper) so the whole graph stays allocation-light and Native-AOT/trim safe.
/// </summary>
internal static class BulkDataMapper
{
	public static Dataset ToDomain(this DatasetDto dto) => new()
	{
		Scope = dto.Scope,
		Name = dto.Name ?? string.Empty,
		Description = dto.Description
	};

	public static IReadOnlyList<Dataset> ToDomain(this DatasetDto[]? dto) =>
		dto is { Length: > 0 } ? Array.ConvertAll(dto, static d => d.ToDomain()) : [];

	public static DatasetFile ToDomain(this DatasetFileDto dto) => new()
	{
		Name = dto.Name ?? string.Empty,
		Size = dto.Size,
		Url = Uri.TryCreate(dto.Url, UriKind.Absolute, out var uri) ? uri : null,
		Sha1 = dto.Sha1,
		Timestamp = ShodanValueParsers.ParseTimestamp(dto.Timestamp)
	};

	public static IReadOnlyList<DatasetFile> ToDomain(this DatasetFileDto[]? dto) =>
		dto is { Length: > 0 } ? Array.ConvertAll(dto, static f => f.ToDomain()) : [];
}
