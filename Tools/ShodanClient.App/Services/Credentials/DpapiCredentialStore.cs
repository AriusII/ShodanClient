using System.Security.Cryptography;
using System.Text.Json;
using ShodanClient.App.Session;

namespace ShodanClient.App.Services.Credentials;

/// <summary>
///     Stores every saved API key profile at <c>%LOCALAPPDATA%\ShodanClient.App\credentials.dat</c>,
///     encrypted as one blob with Windows DPAPI (<see cref="DataProtectionScope.CurrentUser" />) so
///     only the current Windows user account can decrypt it.
/// </summary>
public sealed class DpapiCredentialStore : ICredentialStore, IDisposable
{
	private static readonly string FilePath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		"ShodanClient.App",
		"credentials.dat");

	// Guards read-modify-write cycles against concurrent callers within this process. Cross-process
	// contention isn't a concern: this is a single-instance desktop app.
	private readonly SemaphoreSlim _gate = new(1, 1);

	/// <inheritdoc />
	public async Task<IReadOnlyList<ApiProfileDescriptor>> GetProfilesAsync(
		CancellationToken cancellationToken = default)
	{
		var payload = await ReadPayloadAsync(cancellationToken).ConfigureAwait(true);
		return payload.Profiles.Select(ToDescriptor).ToList();
	}

	/// <inheritdoc />
	public async Task<Guid?> GetActiveProfileIdAsync(CancellationToken cancellationToken = default)
	{
		var payload = await ReadPayloadAsync(cancellationToken).ConfigureAwait(true);
		return payload.ActiveProfileId;
	}

	/// <inheritdoc />
	public async Task<string?> TryGetApiKeyAsync(Guid profileId, CancellationToken cancellationToken = default)
	{
		var payload = await ReadPayloadAsync(cancellationToken).ConfigureAwait(true);
		return payload.Profiles.FirstOrDefault(p => p.Id == profileId)?.ApiKey;
	}

	/// <inheritdoc />
	public async Task<ApiProfileDescriptor> AddProfileAsync(string name, string apiKey,
		CancellationToken cancellationToken = default)
	{
		await _gate.WaitAsync(cancellationToken).ConfigureAwait(true);
		try
		{
			var payload = await ReadPayloadAsync(cancellationToken).ConfigureAwait(true);
			var profile = new StoredProfile
			{
				Id = Guid.NewGuid(),
				Name = name,
				ApiKey = apiKey,
				CreatedAt = DateTimeOffset.UtcNow
			};
			payload.Profiles.Add(profile);
			await WritePayloadAsync(payload, cancellationToken).ConfigureAwait(true);
			return ToDescriptor(profile);
		}
		finally
		{
			_gate.Release();
		}
	}

	/// <inheritdoc />
	public async Task RemoveProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
	{
		await _gate.WaitAsync(cancellationToken).ConfigureAwait(true);
		try
		{
			var payload = await ReadPayloadAsync(cancellationToken).ConfigureAwait(true);
			payload.Profiles.RemoveAll(p => p.Id == profileId);
			if (payload.ActiveProfileId == profileId)
			{
				payload.ActiveProfileId = null;
			}

			await WritePayloadAsync(payload, cancellationToken).ConfigureAwait(true);
		}
		finally
		{
			_gate.Release();
		}
	}

	/// <inheritdoc />
	public async Task RenameProfileAsync(Guid profileId, string newName, CancellationToken cancellationToken = default)
	{
		await _gate.WaitAsync(cancellationToken).ConfigureAwait(true);
		try
		{
			var payload = await ReadPayloadAsync(cancellationToken).ConfigureAwait(true);
			var index = payload.Profiles.FindIndex(p => p.Id == profileId);
			if (index >= 0)
			{
				payload.Profiles[index] = payload.Profiles[index] with { Name = newName };
			}

			await WritePayloadAsync(payload, cancellationToken).ConfigureAwait(true);
		}
		finally
		{
			_gate.Release();
		}
	}

	/// <inheritdoc />
	public async Task SetActiveProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
	{
		await _gate.WaitAsync(cancellationToken).ConfigureAwait(true);
		try
		{
			var payload = await ReadPayloadAsync(cancellationToken).ConfigureAwait(true);
			payload.ActiveProfileId = profileId;
			await WritePayloadAsync(payload, cancellationToken).ConfigureAwait(true);
		}
		finally
		{
			_gate.Release();
		}
	}

	/// <inheritdoc />
	public void Dispose() => _gate.Dispose();

	private static ApiProfileDescriptor ToDescriptor(StoredProfile profile) => new()
	{
		Id = profile.Id,
		Name = profile.Name,
		MaskedKey = ApiProfileDescriptor.Mask(profile.ApiKey),
		CreatedAt = profile.CreatedAt
	};

	private static async Task<CredentialPayload> ReadPayloadAsync(CancellationToken cancellationToken)
	{
		if (!File.Exists(FilePath))
		{
			return new CredentialPayload();
		}

		try
		{
			var protectedBytes = await File.ReadAllBytesAsync(FilePath, cancellationToken).ConfigureAwait(true);
			var jsonBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
			var payload = JsonSerializer.Deserialize(jsonBytes, CredentialJsonContext.Default.CredentialPayload);
			if (payload is null)
			{
				return new CredentialPayload();
			}

			// A file saved before the multi-profile shape existed (or any other payload missing the
			// "profiles" member) deserializes without throwing - System.Text.Json leaves init-only
			// reference-type properties like this one at their CLR default (null) rather than
			// honoring the "= []" initializer when the JSON member is absent. Normalize defensively
			// instead of trusting the deserializer to have populated every member.
			return ReferenceEquals(payload.Profiles, null)
				? new CredentialPayload { ActiveProfileId = payload.ActiveProfileId }
				: payload;
		}
		catch (Exception ex) when (ex is IOException or CryptographicException or JsonException)
		{
			// Covers a corrupt/unreadable file and, deliberately (this app has never shipped, so no
			// migration is implemented), an old payload shape that no longer deserializes: both are
			// treated the same as "no profiles saved yet".
			return new CredentialPayload();
		}
	}

	private static async Task WritePayloadAsync(CredentialPayload payload, CancellationToken cancellationToken)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
		var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(payload, CredentialJsonContext.Default.CredentialPayload);
		var protectedBytes = ProtectedData.Protect(jsonBytes, null, DataProtectionScope.CurrentUser);
		await File.WriteAllBytesAsync(FilePath, protectedBytes, cancellationToken).ConfigureAwait(true);
	}
}
