using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Domain.ApiStatus;

namespace ShodanClient.Infrastructure.ApiStatus;

/// <summary>
///     Logic layer for inspecting the calling account's plan and credit status. A thin validating
///     delegator kept for architectural consistency and as a stable home for future logic.
/// </summary>
internal sealed class ApiStatusService(IApiStatusRepository repository) : IApiStatusService
{
	public Task<ApiInfo> GetAsync(CancellationToken cancellationToken = default) =>
		repository.GetAsync(cancellationToken);
}
