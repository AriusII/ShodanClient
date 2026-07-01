namespace ShodanClient.Infrastructure.Authentication;

/// <summary>
///     Supplies the current Shodan API key. Abstracted so the transport layer never captures a raw
///     key string and so the key can be rotated at runtime (via <c>IOptionsMonitor</c> reloads).
/// </summary>
internal interface IApiKeyProvider
{
	/// <summary>The current API key.</summary>
	string ApiKey { get; }
}
