namespace ShodanClient.Infrastructure.Authentication;

/// <summary>
///     Delegating handler that appends the Shodan API key as a <c>key=</c> query-string parameter to
///     every outgoing request. It is attached ONLY to the typed clients whose surface requires a key
///     (every surface except InternetDB), which is why routes themselves never carry the key.
/// </summary>
internal sealed class ShodanApiKeyHandler(IApiKeyProvider keyProvider) : DelegatingHandler
{
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		var uri = request.RequestUri;
		if (uri is not null && !HasKeyParameter(uri.Query))
		{
			var keyParameter = "key=" + Uri.EscapeDataString(keyProvider.ApiKey);
			var existingQuery = uri.Query; // includes the leading '?', or is empty
			var builder = new UriBuilder(uri)
			{
				// UriBuilder.Query must not include the leading '?'.
				Query = existingQuery.Length > 1
					? string.Concat(existingQuery.AsSpan(1), "&", keyParameter)
					: keyParameter
			};
			request.RequestUri = builder.Uri;
		}

		return base.SendAsync(request, cancellationToken);
	}

	/// <summary>
	///     Defense-in-depth: routes never carry the key by design and this handler is registered to run
	///     once per logical request (outside the resilience retry pipeline), so this should never be
	///     true. It guards against the key being appended twice — and growing without bound on every
	///     retry — if a future change to the handler pipeline ordering ever puts this handler back
	///     downstream of a retrying handler that resends the same <see cref="HttpRequestMessage" />.
	/// </summary>
	private static bool HasKeyParameter(string query)
	{
		if (query.Length < 2)
		{
			return false;
		}

		foreach (var part in query[1..].Split('&'))
		{
			if (part.StartsWith("key=", StringComparison.Ordinal))
			{
				return true;
			}
		}

		return false;
	}
}
