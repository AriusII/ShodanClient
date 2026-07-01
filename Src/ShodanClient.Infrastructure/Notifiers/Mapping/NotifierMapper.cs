using System.Collections.ObjectModel;
using ShodanClient.Domain.Notifiers;
using ShodanClient.Infrastructure.Notifiers.Wire;

namespace ShodanClient.Infrastructure.Notifiers.Mapping;

/// <summary>
///     Maps Notifiers wire DTOs onto pure domain models. Kept as static extension methods (rather than
///     a reflection-based mapper) so the whole graph stays allocation-light and Native-AOT/trim safe.
/// </summary>
internal static class NotifierMapper
{
	private static readonly IReadOnlyDictionary<string, string> EmptyStringMap =
		ReadOnlyDictionary<string, string>.Empty;

	public static Notifier ToDomain(this NotifierDto dto) => new()
	{
		Id = dto.Id ?? string.Empty,
		Provider = dto.Provider ?? string.Empty,
		Description = dto.Description,
		Args = dto.Args ?? EmptyStringMap
	};

	public static IReadOnlyList<Notifier> ToDomain(this NotifierListResponse dto) =>
		dto.Matches is { Length: > 0 }
			? Array.ConvertAll(dto.Matches, static m => m.ToDomain())
			: [];

	public static CreateNotifierResult ToDomain(this CreateNotifierResponse dto) => new()
	{
		Success = dto.Success,
		Id = dto.Id
	};

	public static IReadOnlyList<NotifierProvider> ToDomain(this Dictionary<string, NotifierProviderDto> dto)
	{
		if (dto.Count == 0)
		{
			return [];
		}

		var result = new List<NotifierProvider>(dto.Count);
		foreach (var (name, value) in dto)
		{
			result.Add(new NotifierProvider
			{
				Name = name,
				Required = value.Required ?? []
			});
		}

		return result;
	}
}
