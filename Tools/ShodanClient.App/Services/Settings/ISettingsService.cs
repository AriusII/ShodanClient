namespace ShodanClient.App.Services.Settings;

/// <summary>Loads and persists <see cref="AppSettings" /> as plain JSON on local disk.</summary>
public interface ISettingsService
{
	/// <summary>The current settings, loaded once at startup and kept in memory.</summary>
	AppSettings Current { get; }

	/// <summary>Persists <see cref="Current" /> to disk.</summary>
	Task SaveAsync(CancellationToken cancellationToken = default);
}
