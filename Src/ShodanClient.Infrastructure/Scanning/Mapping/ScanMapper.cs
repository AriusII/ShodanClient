using ShodanClient.Domain.Scanning;
using ShodanClient.Infrastructure.Scanning.Wire;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Scanning.Mapping;

/// <summary>
///     Maps Scanning wire DTOs onto pure domain models. Kept as static extension methods (rather than a
///     reflection-based mapper) so the whole graph stays allocation-light and Native-AOT/trim safe.
/// </summary>
internal static class ScanMapper
{
	public static ScanSubmission ToDomain(this ScanSubmissionResponse dto) => new()
	{
		Id = dto.Id ?? string.Empty,
		Count = dto.Count,
		CreditsLeft = dto.CreditsLeft
	};

	public static ScanInternetResult ToDomain(this ScanInternetResponse dto) => new()
	{
		Id = dto.Id ?? string.Empty
	};

	public static ScanStatus ToDomain(this ScanStatusResponse dto) => new()
	{
		Id = dto.Id ?? string.Empty,
		Status = dto.Status ?? string.Empty,
		Created = ShodanValueParsers.ParseTimestamp(dto.Created),
		Count = dto.Count
	};

	public static IReadOnlyList<ScanListEntry> ToDomain(this ScanListResponse dto) =>
		dto.Matches is { Length: > 0 }
			? Array.ConvertAll(dto.Matches, static m => m.ToDomain())
			: [];

	private static ScanListEntry ToDomain(this ScanListEntryDto dto) => new()
	{
		Id = dto.Id ?? string.Empty,
		Created = ShodanValueParsers.ParseTimestamp(dto.Created),
		Status = dto.Status ?? string.Empty,
		Size = dto.Size,
		CreditsLeft = dto.CreditsLeft
	};
}
