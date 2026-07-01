using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;

namespace ShodanClient.Infrastructure.Utility;

/// <summary>
///     Logic layer for the small diagnostic utilities Shodan exposes. Delegates directly to the
///     repository since these endpoints take no parameters to validate.
/// </summary>
internal sealed class UtilityService(IUtilityRepository repository) : IUtilityService
{
	public Task<IReadOnlyDictionary<string, string>> GetHttpHeadersAsync(
		CancellationToken cancellationToken = default) =>
		repository.GetHttpHeadersAsync(cancellationToken);

	public Task<string> GetMyIpAsync(CancellationToken cancellationToken = default) =>
		repository.GetMyIpAsync(cancellationToken);
}
