using ShodanClient.Domain.ApiStatus;

namespace ShodanClient.Application.Abstractions.Services;

/// <summary>
///     Inspecting the calling account's plan and credit status. Exposed on the client as
///     <c>IShodanClient.ApiInfo</c>.
/// </summary>
public interface IApiStatusService
{
	/// <summary>Fetches the account's plan and credit status (<c>GET /api-info</c>).</summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<ApiInfo> GetAsync(CancellationToken cancellationToken = default);
}
