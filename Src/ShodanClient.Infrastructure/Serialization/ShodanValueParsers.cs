using System.Globalization;

namespace ShodanClient.Infrastructure.Serialization;

/// <summary>
///     Parsing helpers used by the mapping layer to turn Shodan's wire-level strings into strongly
///     typed domain values. Kept out of the JSON converters so parsing stays testable and the wire
///     DTOs remain trivially (de)serializable.
/// </summary>
internal static class ShodanValueParsers
{
	// Shodan timestamps look like "2020-09-29T09:39:45.813661": ISO-8601 with microseconds and
	// NO timezone offset. They are UTC in practice.
	private static readonly string[] TimestampFormats =
	[
		"yyyy-MM-ddTHH:mm:ss.ffffff",
		"yyyy-MM-ddTHH:mm:ss.fff",
		"yyyy-MM-ddTHH:mm:ss"
	];

	/// <summary>
	///     Parses a Shodan timestamp (tz-less, microsecond precision) as a UTC
	///     <see cref="DateTimeOffset" />. Returns <see langword="null" /> for null/blank/unparseable input.
	/// </summary>
	public static DateTimeOffset? ParseTimestamp(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		if (DateTime.TryParseExact(
				value,
				TimestampFormats,
				CultureInfo.InvariantCulture,
				DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
				out var exact))
		{
			return new DateTimeOffset(exact, TimeSpan.Zero);
		}

		// Fall back to a lenient parse for any variant Shodan introduces.
		return DateTimeOffset.TryParse(
			value,
			CultureInfo.InvariantCulture,
			DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
			out var lenient)
			? lenient
			: null;
	}
}
