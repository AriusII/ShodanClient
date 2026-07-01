using ShodanClient.Domain.Account;
using ShodanClient.Infrastructure.Account.Wire;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Account.Mapping;

/// <summary>
///     Maps Account wire DTOs onto pure domain models. Kept as static extension methods (rather than a
///     reflection-based mapper) so the whole graph stays allocation-light and Native-AOT/trim safe.
/// </summary>
internal static class AccountMapper
{
	public static AccountProfile ToDomain(this AccountProfileResponse dto) => new()
	{
		Member = dto.Member,
		Credits = dto.Credits,
		DisplayName = dto.DisplayName,
		Created = ShodanValueParsers.ParseTimestamp(dto.Created)
	};
}
