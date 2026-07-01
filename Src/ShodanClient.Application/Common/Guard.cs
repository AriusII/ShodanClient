using System.Net;
using System.Runtime.CompilerServices;
using ShodanClient.Application.Exceptions;

namespace ShodanClient.Application.Common;

/// <summary>
///     Guard-clause helpers used by the service (logic) layer to validate requests client-side.
///     Every failure throws <see cref="ShodanRequestValidationException" /> so callers can
///     distinguish "you gave me bad input" from "the API said no". Each helper returns the
///     validated value so it can be used fluently.
/// </summary>
internal static class Guard
{
	/// <summary>Ensures a string is non-null and not whitespace.</summary>
	public static string NotNullOrWhiteSpace(
		string? value,
		[CallerArgumentExpression(nameof(value))]
		string? parameterName = null)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ShodanRequestValidationException(
				$"'{parameterName}' must not be null or whitespace.", parameterName ?? "value");
		}

		return value;
	}

	/// <summary>Ensures a collection is non-null and contains at least one element.</summary>
	public static IReadOnlyList<T> NotNullOrEmpty<T>(
		IReadOnlyList<T>? value,
		[CallerArgumentExpression(nameof(value))]
		string? parameterName = null)
	{
		if (value is null || value.Count == 0)
		{
			throw new ShodanRequestValidationException(
				$"'{parameterName}' must contain at least one element.", parameterName ?? "value");
		}

		return value;
	}

	/// <summary>Ensures a dictionary is non-null and contains at least one entry.</summary>
	public static IReadOnlyDictionary<TKey, TValue> NotNullOrEmpty<TKey, TValue>(
		IReadOnlyDictionary<TKey, TValue>? value,
		[CallerArgumentExpression(nameof(value))]
		string? parameterName = null)
		where TKey : notnull
	{
		if (value is null || value.Count == 0)
		{
			throw new ShodanRequestValidationException(
				$"'{parameterName}' must contain at least one entry.", parameterName ?? "value");
		}

		return value;
	}

	/// <summary>Ensures an integer is greater than or equal to <paramref name="min" />.</summary>
	public static int AtLeast(
		int value,
		int min,
		[CallerArgumentExpression(nameof(value))]
		string? parameterName = null)
	{
		if (value < min)
		{
			throw new ShodanRequestValidationException(
				$"'{parameterName}' must be greater than or equal to {min} but was {value}.", parameterName ?? "value");
		}

		return value;
	}

	/// <summary>Ensures a value parses as a valid IPv4 or IPv6 address.</summary>
	public static string ValidIpAddress(
		string? value,
		[CallerArgumentExpression(nameof(value))]
		string? parameterName = null)
	{
		NotNullOrWhiteSpace(value, parameterName);
		if (!IPAddress.TryParse(value, out _))
		{
			throw new ShodanRequestValidationException(
				$"'{parameterName}' must be a valid IP address but was '{value}'.", parameterName ?? "value");
		}

		return value;
	}
}
