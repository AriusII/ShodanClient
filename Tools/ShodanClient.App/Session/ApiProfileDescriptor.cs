namespace ShodanClient.App.Session;

/// <summary>
///     A saved Shodan API key profile the user can switch between at runtime. Never carries the raw
///     key: <see cref="MaskedKey" /> is safe to bind directly to the UI, the real key only ever lives
///     inside the encrypted credential store.
/// </summary>
public sealed record ApiProfileDescriptor
{
	/// <summary>A stable identifier for this profile, independent of its display name.</summary>
	public required Guid Id { get; init; }

	/// <summary>The user-chosen display name, e.g. <c>"Work"</c> or <c>"Personal"</c>.</summary>
	public required string Name { get; init; }

	/// <summary>The first 4 and last 4 characters of the real key, with the middle redacted.</summary>
	public required string MaskedKey { get; init; }

	/// <summary>When this profile was first added.</summary>
	public required DateTimeOffset CreatedAt { get; init; }

	/// <summary>Masks <paramref name="apiKey" /> as <c>first4••••••last4</c>, for display only.</summary>
	public static string Mask(string apiKey)
	{
		if (string.IsNullOrEmpty(apiKey))
		{
			return string.Empty;
		}

		return apiKey.Length <= 8
			? new string('•', apiKey.Length)
			: $"{apiKey[..4]}••••••{apiKey[^4..]}";
	}
}
