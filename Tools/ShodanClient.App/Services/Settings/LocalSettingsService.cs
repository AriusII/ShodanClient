using System.Text.Json;

namespace ShodanClient.App.Services.Settings;

/// <summary>
///     Default <see cref="ISettingsService" />: reads/writes <c>%LOCALAPPDATA%\ShodanClient.App\settings.json</c>.
/// </summary>
public sealed class LocalSettingsService : ISettingsService
{
	private static readonly string FilePath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		"ShodanClient.App",
		"settings.json");

	/// <summary>Creates the settings service, eagerly loading any previously saved settings.</summary>
	public LocalSettingsService()
	{
		Current = Load();
	}

	/// <inheritdoc />
	public AppSettings Current { get; }

	/// <inheritdoc />
	public async Task SaveAsync(CancellationToken cancellationToken = default)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
		await using var stream = File.Create(FilePath);
		await JsonSerializer.SerializeAsync(stream, Current, SettingsJsonContext.Default.AppSettings, cancellationToken)
			.ConfigureAwait(true);
	}

	private static AppSettings Load()
	{
		if (!File.Exists(FilePath))
		{
			return new AppSettings();
		}

		try
		{
			using var stream = File.OpenRead(FilePath);
			return JsonSerializer.Deserialize(stream, SettingsJsonContext.Default.AppSettings) ?? new AppSettings();
		}
		catch (Exception ex) when (ex is IOException or JsonException)
		{
			return new AppSettings();
		}
	}
}
