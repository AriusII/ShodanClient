namespace ShodanClient.Application.Exceptions;

/// <summary>
///     Thrown by the client BEFORE any network call when a request fails client-side validation
///     (for example a missing query, an invalid IP address, or an out-of-range page number).
///     These never leave the process and never consume Shodan credits.
/// </summary>
public sealed class ShodanRequestValidationException : ShodanException
{
	/// <summary>Creates a validation exception.</summary>
	public ShodanRequestValidationException(string message)
		: base(message)
	{
	}

	/// <summary>Creates a validation exception naming the offending parameter.</summary>
	public ShodanRequestValidationException(string message, string parameterName)
		: base(message)
	{
		ParameterName = parameterName;
	}

	/// <summary>The name of the parameter that failed validation, if known.</summary>
	public string? ParameterName { get; }
}
