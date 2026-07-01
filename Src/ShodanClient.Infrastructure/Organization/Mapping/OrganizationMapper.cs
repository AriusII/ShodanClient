using ShodanClient.Domain.Organization;
using ShodanClient.Infrastructure.Organization.Wire;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Organization.Mapping;

// NOTE: the domain type is referenced by its fully qualified name below rather than a `using` +
// bare `Organization`, because this file's enclosing namespace chain passes through
// `ShodanClient.Infrastructure.Organization` itself; an unqualified `Organization` would bind to
// that enclosing namespace and fail with CS0118.

/// <summary>
///     Maps Organization wire DTOs onto pure domain models. Kept as static extension methods (rather
///     than a reflection-based mapper) so the whole graph stays allocation-light and Native-AOT/trim safe.
/// </summary>
internal static class OrganizationMapper
{
	public static Domain.Organization.Organization ToDomain(this OrganizationResponse dto) => new()
	{
		Id = dto.Id,
		Name = dto.Name,
		Created = ShodanValueParsers.ParseTimestamp(dto.Created),
		Admins = ToDomain(dto.Admins),
		Members = ToDomain(dto.Members),
		UpgradeType = dto.UpgradeType,
		Domains = dto.Domains ?? [],
		Logo = dto.Logo
	};

	private static OrganizationMember ToDomain(this OrganizationMemberDto dto) => new()
	{
		Username = dto.Username ?? string.Empty,
		Email = dto.Email
	};

	private static OrganizationMember[] ToDomain(OrganizationMemberDto[]? dto) =>
		dto is { Length: > 0 } ? Array.ConvertAll(dto, static m => m.ToDomain()) : [];
}
