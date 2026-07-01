using System.Collections.ObjectModel;
using ShodanClient.Domain.Alerts;
using ShodanClient.Infrastructure.Alerts.Wire;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Alerts.Mapping;

/// <summary>
///     Maps Alerts wire DTOs onto pure domain models. Kept as static extension methods (rather than a
///     reflection-based mapper) so the whole graph stays allocation-light and Native-AOT/trim safe.
/// </summary>
internal static class AlertMapper
{
	private static readonly IReadOnlyDictionary<string, AlertTrigger> EmptyTriggers =
		ReadOnlyDictionary<string, AlertTrigger>.Empty;

	public static Alert ToDomain(this AlertDto dto) => new()
	{
		Id = dto.Id ?? string.Empty,
		Name = dto.Name ?? string.Empty,
		Created = ShodanValueParsers.ParseTimestamp(dto.Created),
		Expires = dto.Expires,
		Size = dto.Size,
		Filters = dto.Filters?.ToDomain() ?? new AlertFilters(),
		Triggers = ToTriggerMap(dto.Triggers),
		HasTriggers = dto.HasTriggers,
		Expiration = dto.Expiration,
		Notifiers = dto.Notifiers is { Length: > 0 }
			? Array.ConvertAll(dto.Notifiers, static n => n.ToDomain())
			: []
	};

	public static IReadOnlyList<Alert> ToDomain(this AlertDto[]? dto) =>
		dto is { Length: > 0 } ? Array.ConvertAll(dto, static a => a.ToDomain()) : [];

	public static IReadOnlyList<TriggerDefinition> ToDomain(this TriggerDefinitionDto[]? dto) =>
		dto is { Length: > 0 }
			? Array.ConvertAll(dto, static t => new TriggerDefinition
			{
				Name = t.Name ?? string.Empty,
				Rule = t.Rule,
				Description = t.Description
			})
			: [];

	private static AlertFilters ToDomain(this AlertFiltersDto dto) => new()
	{
		Ip = dto.Ip ?? []
	};

	private static AlertNotifierRef ToDomain(this AlertNotifierRefDto dto) => new()
	{
		Id = dto.Id ?? string.Empty,
		Provider = dto.Provider
	};

	private static IReadOnlyDictionary<string, AlertTrigger> ToTriggerMap(Dictionary<string, AlertTriggerDto>? source)
	{
		if (source is null || source.Count == 0)
		{
			return EmptyTriggers;
		}

		var result = new Dictionary<string, AlertTrigger>(source.Count, StringComparer.OrdinalIgnoreCase);
		foreach (var (key, value) in source)
		{
			result[key] = new AlertTrigger { Rule = value.Rule };
		}

		return result;
	}
}
