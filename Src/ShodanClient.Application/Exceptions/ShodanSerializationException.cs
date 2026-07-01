namespace ShodanClient.Application.Exceptions;

/// <summary>
///     Thrown when a Shodan response could not be read into the expected shape — for example an
///     empty body where an object was expected, or malformed JSON. Usually indicates an upstream
///     API change; the underlying parser error is preserved as the inner exception.
/// </summary>
public sealed class ShodanSerializationException : ShodanException
{
	/// <summary>Creates a serialization exception.</summary>
	public ShodanSerializationException(string message)
		: base(message)
	{
	}

	/// <summary>Creates a serialization exception with the underlying parser error.</summary>
	public ShodanSerializationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
