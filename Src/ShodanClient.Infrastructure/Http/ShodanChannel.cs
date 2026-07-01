using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using ShodanClient.Application.Exceptions;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Http;

/// <summary>
///     Shared transport for a single Shodan surface. Owns the typed <see cref="HttpClient" /> (whose
///     base address is that surface's host) and is the ONLY place that speaks HTTP: it sends requests,
///     deserializes JSON via the source-generated context, streams NDJSON, and translates non-success
///     responses into typed <see cref="ShodanApiException" />s. Repositories call it with pure
///     <see cref="ShodanRoute" />s and <see cref="JsonTypeInfo{T}" />s.
/// </summary>
internal abstract class ShodanChannel(HttpClient httpClient)
{
	/// <summary>The surface this channel serves (used for diagnostics and error context).</summary>
	protected abstract ShodanApiSurface Surface { get; }

	private string SurfaceName => Surface.ToString();

	/// <summary>Issues a GET and deserializes the JSON body to <typeparamref name="T" />.</summary>
	public async Task<T> GetJsonAsync<T>(ShodanRoute route, JsonTypeInfo<T> typeInfo,
		CancellationToken cancellationToken)
	{
		using var request = new HttpRequestMessage(route.Method, route.RelativePath);
		using var response =
			await SendCoreAsync(request, HttpCompletionOption.ResponseContentRead, route, cancellationToken)
				.ConfigureAwait(false);
		return await ReadJsonAsync(response, route, typeInfo, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>Sends a request with a <c>application/x-www-form-urlencoded</c> body and deserializes the response.</summary>
	public async Task<T> SendFormAsync<T>(
		ShodanRoute route,
		IEnumerable<KeyValuePair<string, string>> form,
		JsonTypeInfo<T> typeInfo,
		CancellationToken cancellationToken)
	{
		using var content = new FormUrlEncodedContent(form);
		using var request = new HttpRequestMessage(route.Method, route.RelativePath) { Content = content };
		using var response =
			await SendCoreAsync(request, HttpCompletionOption.ResponseContentRead, route, cancellationToken)
				.ConfigureAwait(false);
		return await ReadJsonAsync(response, route, typeInfo, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>Sends a request with a JSON body and deserializes the JSON response.</summary>
	public async Task<TResponse> SendJsonAsync<TBody, TResponse>(
		ShodanRoute route,
		TBody body,
		JsonTypeInfo<TBody> bodyTypeInfo,
		JsonTypeInfo<TResponse> responseTypeInfo,
		CancellationToken cancellationToken)
	{
		using var content = JsonContent.Create(body, bodyTypeInfo);
		using var request = new HttpRequestMessage(route.Method, route.RelativePath) { Content = content };
		using var response =
			await SendCoreAsync(request, HttpCompletionOption.ResponseContentRead, route, cancellationToken)
				.ConfigureAwait(false);
		return await ReadJsonAsync(response, route, responseTypeInfo, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	///     Sends a request (optionally with a body) that returns only an acknowledgement, and reports
	///     whether it succeeded. A <c>{ "success": false }</c> body maps to <see langword="false" />;
	///     any 2xx with no explicit flag maps to <see langword="true" />.
	/// </summary>
	public async Task<bool> SendForSuccessAsync(ShodanRoute route, HttpContent? content,
		CancellationToken cancellationToken)
	{
		using var request = new HttpRequestMessage(route.Method, route.RelativePath) { Content = content };
		using var response =
			await SendCoreAsync(request, HttpCompletionOption.ResponseContentRead, route, cancellationToken)
				.ConfigureAwait(false);

		try
		{
			var ack = await response.Content
				.ReadFromJsonAsync(ShodanJsonContext.Default.ShodanSuccessResponse, cancellationToken)
				.ConfigureAwait(false);
			return ack?.Success ?? true;
		}
		catch (JsonException)
		{
			// Some write endpoints return an empty or non-JSON body on success.
			return true;
		}
		catch (NotSupportedException)
		{
			return true;
		}
	}

	/// <summary>
	///     Opens a long-lived streaming response and yields each newline-delimited JSON object as it
	///     arrives. The response headers are read eagerly (<see cref="HttpCompletionOption.ResponseHeadersRead" />)
	///     so items surface without buffering the whole (unbounded) stream.
	/// </summary>
	public async IAsyncEnumerable<T> StreamNdjsonAsync<T>(
		ShodanRoute route,
		JsonTypeInfo<T> typeInfo,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		using var request = new HttpRequestMessage(route.Method, route.RelativePath);
		using var response =
			await SendCoreAsync(request, HttpCompletionOption.ResponseHeadersRead, route, cancellationToken)
				.ConfigureAwait(false);

		var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
		await using var disposeStream = stream.ConfigureAwait(false);

		var sequence = JsonSerializer.DeserializeAsyncEnumerable(stream, typeInfo, true, cancellationToken);
		await foreach (var item in sequence.ConfigureAwait(false))
		{
			if (item is not null)
			{
				yield return item;
			}
		}
	}

	private async Task<HttpResponseMessage> SendCoreAsync(
		HttpRequestMessage request,
		HttpCompletionOption completionOption,
		ShodanRoute route,
		CancellationToken cancellationToken)
	{
		HttpResponseMessage response;
		try
		{
			response = await httpClient.SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
		}
		catch (HttpRequestException ex)
		{
			throw new ShodanApiException(
				ex.StatusCode ?? HttpStatusCode.ServiceUnavailable,
				ex.Message,
				RelativeUri(route),
				SurfaceName,
				ex);
		}
		catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
		{
			throw new ShodanApiException(
				HttpStatusCode.RequestTimeout,
				"The request timed out.",
				RelativeUri(route),
				SurfaceName,
				ex);
		}

		if (response.IsSuccessStatusCode)
		{
			return response;
		}

		if (IsRedirect(response.StatusCode))
		{
			var location = response.Headers.Location;
			var redirectStatusCode = response.StatusCode;
			response.Dispose();
			throw new ShodanApiException(
				redirectStatusCode,
				location is not null
					? $"The endpoint unexpectedly redirected to '{RedactApiKey(location)}' instead of returning data directly. " +
					  "This usually means Shodan has moved or retired the endpoint upstream; this client does not " +
					  "follow such redirects automatically because the target may use an incompatible response schema."
					: "The endpoint returned an unexpected redirect response instead of returning data directly.",
				RelativeUri(route),
				SurfaceName);
		}

		var (message, retryAfter) = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
		var statusCode = response.StatusCode;
		response.Dispose();
		throw ShodanErrorTranslator.Translate(statusCode, message, RelativeUri(route), SurfaceName, retryAfter);
	}

	private async Task<T> ReadJsonAsync<T>(
		HttpResponseMessage response,
		ShodanRoute route,
		JsonTypeInfo<T> typeInfo,
		CancellationToken cancellationToken)
	{
		try
		{
			var value = await response.Content.ReadFromJsonAsync(typeInfo, cancellationToken).ConfigureAwait(false);
			return value ?? throw new ShodanSerializationException(
				$"The Shodan {SurfaceName} response for '{route.RelativePath}' was empty or null.");
		}
		catch (JsonException ex)
		{
			throw new ShodanSerializationException(
				$"Failed to read the Shodan {SurfaceName} response for '{route.RelativePath}'.", ex);
		}
		catch (NotSupportedException ex)
		{
			throw new ShodanSerializationException(
				$"The Shodan {SurfaceName} response for '{route.RelativePath}' had an unexpected content type.", ex);
		}
	}

	private static async Task<(string? Message, TimeSpan? RetryAfter)> ReadErrorAsync(
		HttpResponseMessage response,
		CancellationToken cancellationToken)
	{
		string? message = null;
		try
		{
			var error = await response.Content
				.ReadFromJsonAsync(ShodanJsonContext.Default.ShodanErrorResponse, cancellationToken)
				.ConfigureAwait(false);
			message = error?.Message;
		}
		catch (JsonException)
		{
			// Non-JSON error body (e.g. an HTML error page); leave the message null.
		}
		catch (NotSupportedException)
		{
		}

		var retryAfter = ParseRetryAfter(response.Headers.RetryAfter);
		return (message, retryAfter);
	}

	/// <summary>
	///     <c>Retry-After</c> (RFC 7231 §7.1.3) may arrive as delta-seconds or as an HTTP-date; only
	///     one of <see cref="RetryConditionHeaderValue.Delta" />/<see cref="RetryConditionHeaderValue.Date" />
	///     is populated depending on which form the server used.
	/// </summary>
	private static TimeSpan? ParseRetryAfter(RetryConditionHeaderValue? header) => header switch
	{
		{ Delta: { } delta } => delta,
		{ Date: { } date } => date - DateTimeOffset.UtcNow,
		_ => null
	};

	private static Uri RelativeUri(ShodanRoute route) => new(route.RelativePath, UriKind.Relative);

	private static bool IsRedirect(HttpStatusCode statusCode) => statusCode is
		HttpStatusCode.MovedPermanently or
		HttpStatusCode.Redirect or
		HttpStatusCode.SeeOther or
		HttpStatusCode.RedirectKeepVerb or
		HttpStatusCode.PermanentRedirect;

	/// <summary>
	///     Strips any <c>key=</c> query parameter from a URI before it is interpolated into an
	///     exception message. Every request to a key-required surface carries the caller's raw Shodan
	///     API key in its query string (see <see cref="Authentication.ShodanApiKeyHandler" />); if a
	///     redirect's <c>Location</c> header ever echoes back the original query, this keeps the key
	///     out of exception messages (and therefore out of anything that logs them) the same way
	///     <see cref="RelativeUri" /> already keeps the key out of <see cref="ShodanApiException.RequestUri" />.
	/// </summary>
	private static string RedactApiKey(Uri uri)
	{
		var text = uri.IsAbsoluteUri ? uri.ToString() : uri.OriginalString;
		var queryStart = text.IndexOf('?');
		if (queryStart < 0)
		{
			return text;
		}

		var fragmentStart = text.IndexOf('#', queryStart);
		var queryEnd = fragmentStart < 0 ? text.Length : fragmentStart;
		var query = text[(queryStart + 1)..queryEnd];
		var filtered = string.Join('&',
			query.Split('&').Where(static part => !part.StartsWith("key=", StringComparison.OrdinalIgnoreCase)));

		return filtered.Length == 0
			? string.Concat(text.AsSpan(0, queryStart), text.AsSpan(queryEnd))
			: string.Concat(text.AsSpan(0, queryStart), "?", filtered, text.AsSpan(queryEnd));
	}
}
