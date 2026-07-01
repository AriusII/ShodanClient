using ShodanClient.Domain.InternetDb;
using ShodanClient.Infrastructure.InternetDb.Wire;

namespace ShodanClient.Infrastructure.InternetDb.Mapping;

/// <summary>Maps InternetDB wire DTOs onto domain models.</summary>
internal static class InternetDbMapper
{
	public static InternetDbHost ToDomain(this InternetDbResponse dto) => new()
	{
		Ip = dto.Ip ?? string.Empty,
		Ports = dto.Ports ?? [],
		Cpes = dto.Cpes ?? [],
		Hostnames = dto.Hostnames ?? [],
		Tags = dto.Tags ?? [],
		Vulnerabilities = dto.Vulns ?? []
	};
}
