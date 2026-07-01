using System.Net;
using ShodanClient.Application.Exceptions;

namespace ShodanClient.Application.Tests;

/// <summary>
///     Unit coverage for the <see cref="ShodanException" /> hierarchy — the base type, the
///     generic <see cref="ShodanApiException" /> and its status-code-specific subtypes, and the
///     two client-side exceptions (<see cref="ShodanRequestValidationException" />,
///     <see cref="ShodanSerializationException" />) that never touch the network.
/// </summary>
public sealed class ExceptionTests
{
	[Fact]
	public void ShodanException_is_abstract() => Assert.True(typeof(ShodanException).IsAbstract);

	// ----- ShodanApiException (base) -----------------------------------------------------

	[Fact]
	public void ShodanApiException_sets_properties_from_constructor_arguments()
	{
		var requestUri = new Uri("https://api.shodan.io/shodan/host/8.8.8.8");
		var inner = new InvalidOperationException("boom");

		var ex = new ShodanApiException(HttpStatusCode.BadGateway, "upstream failure", requestUri, "Rest", inner);

		Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
		Assert.Equal("upstream failure", ex.ApiMessage);
		Assert.Same(requestUri, ex.RequestUri);
		Assert.Equal("Rest", ex.Surface);
		Assert.Same(inner, ex.InnerException);
	}

	[Fact]
	public void ShodanApiException_is_a_ShodanException()
	{
		var ex = new ShodanApiException(HttpStatusCode.BadRequest);

		Assert.IsAssignableFrom<ShodanException>(ex);
	}

	[Fact]
	public void ShodanApiException_defaults_are_null_when_optional_arguments_omitted()
	{
		var ex = new ShodanApiException(HttpStatusCode.BadRequest);

		Assert.Null(ex.ApiMessage);
		Assert.Null(ex.RequestUri);
		Assert.Null(ex.Surface);
		Assert.Null(ex.InnerException);
	}

	[Fact]
	public void ShodanApiException_message_without_apiMessage_or_surface_omits_both()
	{
		var ex = new ShodanApiException(HttpStatusCode.BadRequest);

		Assert.Equal("Shodan API request failed with status 400 (BadRequest).", ex.Message);
	}

	[Fact]
	public void ShodanApiException_message_with_apiMessage_appends_it_after_a_colon()
	{
		var ex = new ShodanApiException(HttpStatusCode.BadRequest, "invalid query");

		Assert.Equal("Shodan API request failed with status 400 (BadRequest): invalid query", ex.Message);
	}

	[Fact]
	public void ShodanApiException_message_with_surface_includes_surface_name()
	{
		var ex = new ShodanApiException(HttpStatusCode.BadRequest, surface: "Streaming");

		Assert.Equal("Shodan Streaming API request failed with status 400 (BadRequest).", ex.Message);
	}

	[Fact]
	public void ShodanApiException_message_with_empty_apiMessage_is_treated_as_absent()
	{
		var ex = new ShodanApiException(HttpStatusCode.BadRequest, string.Empty);

		Assert.Equal("Shodan API request failed with status 400 (BadRequest).", ex.Message);
	}

	// ----- ShodanAuthenticationException (401) --------------------------------------------

	[Fact]
	public void ShodanAuthenticationException_uses_401_and_is_an_api_exception()
	{
		var ex = new ShodanAuthenticationException("bad key");

		Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
		Assert.Equal("bad key", ex.ApiMessage);
		Assert.IsAssignableFrom<ShodanApiException>(ex);
		Assert.Equal("Shodan API request failed with status 401 (Unauthorized): bad key", ex.Message);
	}

	[Fact]
	public void ShodanAuthenticationException_without_apiMessage_has_generic_message()
	{
		var ex = new ShodanAuthenticationException();

		Assert.Equal("Shodan API request failed with status 401 (Unauthorized).", ex.Message);
	}

	// ----- ShodanAccessDeniedException (403) -----------------------------------------------

	[Fact]
	public void ShodanAccessDeniedException_uses_403_and_is_an_api_exception()
	{
		var ex = new ShodanAccessDeniedException("plan does not include this endpoint");

		Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
		Assert.Equal("plan does not include this endpoint", ex.ApiMessage);
		Assert.IsAssignableFrom<ShodanApiException>(ex);
		Assert.Equal("Shodan API request failed with status 403 (Forbidden): plan does not include this endpoint",
			ex.Message);
	}

	[Fact]
	public void ShodanAccessDeniedException_without_apiMessage_has_generic_message()
	{
		var ex = new ShodanAccessDeniedException();

		Assert.Equal("Shodan API request failed with status 403 (Forbidden).", ex.Message);
	}

	// ----- ShodanNotFoundException (404) ---------------------------------------------------

	[Fact]
	public void ShodanNotFoundException_uses_404_and_is_an_api_exception()
	{
		var ex = new ShodanNotFoundException("no information available");

		Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
		Assert.Equal("no information available", ex.ApiMessage);
		Assert.IsAssignableFrom<ShodanApiException>(ex);
		Assert.Equal("Shodan API request failed with status 404 (NotFound): no information available", ex.Message);
	}

	[Fact]
	public void ShodanNotFoundException_without_apiMessage_has_generic_message()
	{
		var ex = new ShodanNotFoundException();

		Assert.Equal("Shodan API request failed with status 404 (NotFound).", ex.Message);
	}

	// ----- ShodanRateLimitException (429) ---------------------------------------------------

	[Fact]
	public void ShodanRateLimitException_uses_429_and_is_an_api_exception()
	{
		var ex = new ShodanRateLimitException(apiMessage: "rate limit exceeded");

		Assert.Equal(HttpStatusCode.TooManyRequests, ex.StatusCode);
		Assert.Equal("rate limit exceeded", ex.ApiMessage);
		Assert.IsAssignableFrom<ShodanApiException>(ex);
		Assert.Equal("Shodan API request failed with status 429 (TooManyRequests): rate limit exceeded", ex.Message);
	}

	[Fact]
	public void ShodanRateLimitException_without_apiMessage_has_generic_message()
	{
		var ex = new ShodanRateLimitException();

		Assert.Equal("Shodan API request failed with status 429 (TooManyRequests).", ex.Message);
	}

	[Fact]
	public void ShodanRateLimitException_retryAfter_round_trips_when_provided()
	{
		var retryAfter = TimeSpan.FromSeconds(30);

		var ex = new ShodanRateLimitException(retryAfter);

		Assert.Equal(retryAfter, ex.RetryAfter);
	}

	[Fact]
	public void ShodanRateLimitException_retryAfter_is_null_when_not_provided()
	{
		var ex = new ShodanRateLimitException();

		Assert.Null(ex.RetryAfter);
	}

	[Fact]
	public void ShodanRateLimitException_retryAfter_is_explicitly_null_when_passed_null()
	{
		var ex = new ShodanRateLimitException();

		Assert.Null(ex.RetryAfter);
	}

	// ----- ShodanServerException (5xx) ------------------------------------------------------

	[Fact]
	public void ShodanServerException_defaults_to_internal_server_error()
	{
		var ex = new ShodanServerException();

		Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
		Assert.IsAssignableFrom<ShodanApiException>(ex);
		Assert.Equal("Shodan API request failed with status 500 (InternalServerError).", ex.Message);
	}

	[Fact]
	public void ShodanServerException_accepts_a_custom_5xx_status_code()
	{
		var ex = new ShodanServerException(HttpStatusCode.BadGateway, "upstream timeout");

		Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
		Assert.Equal("upstream timeout", ex.ApiMessage);
		Assert.Equal("Shodan API request failed with status 502 (BadGateway): upstream timeout", ex.Message);
	}

	// ----- ShodanRequestValidationException (client-side) -----------------------------------

	[Fact]
	public void ShodanRequestValidationException_is_a_ShodanException_but_not_an_api_exception()
	{
		var ex = new ShodanRequestValidationException("query must not be null or whitespace");

		Assert.IsAssignableFrom<ShodanException>(ex);
		Assert.IsNotType<ShodanApiException>(ex, false);
	}

	[Fact]
	public void ShodanRequestValidationException_message_only_constructor_leaves_parameterName_null()
	{
		var ex = new ShodanRequestValidationException("query must not be null or whitespace");

		Assert.Equal("query must not be null or whitespace", ex.Message);
		Assert.Null(ex.ParameterName);
	}

	[Fact]
	public void ShodanRequestValidationException_with_parameterName_captures_it_without_altering_message()
	{
		var ex = new ShodanRequestValidationException("must be a valid IP address", "ip");

		Assert.Equal("must be a valid IP address", ex.Message);
		Assert.Equal("ip", ex.ParameterName);
	}

	// ----- ShodanSerializationException (client-side) ---------------------------------------

	[Fact]
	public void ShodanSerializationException_is_a_ShodanException_but_not_an_api_exception()
	{
		var ex = new ShodanSerializationException("could not deserialize the response body");

		Assert.IsAssignableFrom<ShodanException>(ex);
		Assert.IsNotType<ShodanApiException>(ex, false);
	}

	[Fact]
	public void ShodanSerializationException_message_only_constructor_leaves_innerException_null()
	{
		var ex = new ShodanSerializationException("could not deserialize the response body");

		Assert.Equal("could not deserialize the response body", ex.Message);
		Assert.Null(ex.InnerException);
	}

	[Fact]
	public void ShodanSerializationException_with_innerException_preserves_the_parser_error()
	{
		var parserError = new FormatException("unexpected token");

		var ex = new ShodanSerializationException("malformed JSON", parserError);

		Assert.Equal("malformed JSON", ex.Message);
		Assert.Same(parserError, ex.InnerException);
	}
}
