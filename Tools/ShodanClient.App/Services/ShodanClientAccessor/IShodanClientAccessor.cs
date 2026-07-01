using ShodanClient.App.Services.Settings;

namespace ShodanClient.App.Services.ShodanClientAccessor;

/// <summary>
///     Owns the SDK's <see cref="IShodanClient" /> lifetime independently of the app's main DI
///     container, so the app can start without a key, rotate the key at runtime, and surface
///     configuration failures without crashing. View models depend on this, never on
///     <see cref="IShodanClient" /> directly.
/// </summary>
public interface IShodanClientAccessor
{
	/// <summary>The active client, or <see langword="null" /> if no valid key is attached.</summary>
	IShodanClient? Client { get; }

	/// <summary>Whether a validated client is currently attached.</summary>
	bool IsConfigured { get; }

	/// <summary>Raised whenever <see cref="Client" />/<see cref="IsConfigured" /> changes (attach or detach).</summary>
	event Action? Changed;

	/// <summary>
	///     Builds a new SDK client for <paramref name="apiKey" />, validates it with a free-tier
	///     network call, and attaches it on success. Any previously attached client is disposed first.
	/// </summary>
	/// <returns><see langword="true" /> if the key was accepted and the client attached.</returns>
	Task<bool> TryAttachAsync(string apiKey, AppSettings settings, CancellationToken cancellationToken = default);

	/// <summary>Detaches and disposes the current client, if any.</summary>
	Task DetachAsync();

	/// <summary>
	///     A permanently available client for keyless surfaces (currently just InternetDB),
	///     independent of whether a real API key has ever been configured. Built lazily on first use
	///     and cached for the app's lifetime, so the "free, key-less" InternetDB lookup genuinely
	///     never requires an API key, even before Setup has completed.
	/// </summary>
	Task<IShodanClient> GetAnonymousClientAsync(CancellationToken cancellationToken = default);
}
