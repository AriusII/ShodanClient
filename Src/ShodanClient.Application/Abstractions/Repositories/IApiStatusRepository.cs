using ShodanClient.Domain.ApiStatus;

namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to the API status endpoint on the REST API.</summary>
internal interface IApiStatusRepository
{
	/// <summary>Fetches the account's plan and credit status (<c>GET /api-info</c>).</summary>
	Task<ApiInfo> GetAsync(CancellationToken cancellationToken);
}
