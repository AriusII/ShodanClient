using Microsoft.Extensions.Options;
using ShodanClient.Application.Configuration;

namespace ShodanClient.Infrastructure.Authentication;

/// <summary>
///     Resolves the current API key from <see cref="ShodanClientOptions" /> via
///     <see cref="IOptionsMonitor{TOptions}" />, so key rotation through configuration reloads is
///     picked up without restarting the process.
/// </summary>
internal sealed class ApiKeyProvider(IOptionsMonitor<ShodanClientOptions> options) : IApiKeyProvider
{
	/// <inheritdoc />
	public string ApiKey => options.CurrentValue.ApiKey;
}
