namespace ShodanClient.Application.Exceptions;

/// <summary>
///     Base type for every exception thrown by ShodanClient. Catch this to handle any
///     library-originated failure regardless of whether it came from the network, the
///     server, deserialization, or client-side validation.
/// </summary>
public abstract class ShodanException : Exception
{
	/// <summary>Initializes a new instance.</summary>
	protected ShodanException()
	{
	}

	/// <summary>Initializes a new instance with a message.</summary>
	protected ShodanException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance with a message and inner exception.</summary>
	protected ShodanException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
