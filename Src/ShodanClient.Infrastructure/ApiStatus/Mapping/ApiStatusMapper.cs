using ShodanClient.Domain.ApiStatus;
using ShodanClient.Infrastructure.ApiStatus.Wire;

namespace ShodanClient.Infrastructure.ApiStatus.Mapping;

/// <summary>
///     Maps API-status wire DTOs onto pure domain models. Kept as static extension methods (rather
///     than a reflection-based mapper) so the whole graph stays allocation-light and Native-AOT/trim
///     safe.
/// </summary>
internal static class ApiStatusMapper
{
	public static ApiInfo ToDomain(this ApiInfoResponse dto) => new()
	{
		ScanCredits = dto.ScanCredits,
		QueryCredits = dto.QueryCredits,
		MonitoredIps = dto.MonitoredIps,
		Plan = dto.Plan ?? string.Empty,
		Https = dto.Https,
		Telnet = dto.Telnet,
		Unlocked = dto.Unlocked,
		UnlockedLeft = dto.UnlockedLeft,
		UsageLimits = dto.UsageLimits?.ToDomain() ?? new UsageLimits { ScanCredits = 0, QueryCredits = 0 }
	};

	private static UsageLimits ToDomain(this UsageLimitsDto dto) => new()
	{
		ScanCredits = dto.ScanCredits,
		QueryCredits = dto.QueryCredits,
		MonitoredIps = dto.MonitoredIps
	};
}
