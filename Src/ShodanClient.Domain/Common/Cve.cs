using System.Diagnostics.CodeAnalysis;

namespace ShodanClient.Domain.Common;

/// <summary>
///     A CVE identifier such as <c>CVE-2021-44228</c>. Immutable value object with
///     case-insensitive equality (the identifier is normalized to upper-case).
/// </summary>
public readonly record struct Cve
{
	private readonly string? _value;

	/// <summary>Creates a normalized CVE identifier.</summary>
	/// <param name="value">A CVE identifier, e.g. <c>CVE-2021-44228</c>.</param>
	/// <exception cref="ArgumentException">The value is null or whitespace.</exception>
	public Cve(string value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(value);
		_value = value.Trim().ToUpperInvariant();
	}

	/// <summary>The normalized identifier, or an empty string for a default instance.</summary>
	public string Value => _value ?? string.Empty;

	/// <summary>Whether this instance holds an identifier (i.e. is not <c>default</c>).</summary>
	public bool HasValue => _value is not null;

	/// <summary>Parses a CVE identifier.</summary>
	public static Cve Parse(string value) => new(value);

	/// <summary>Attempts to parse a CVE identifier without throwing.</summary>
	public static bool TryParse([NotNullWhen(true)] string? value, out Cve cve)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			cve = default;
			return false;
		}

		cve = new Cve(value);
		return true;
	}

	/// <inheritdoc />
	public override string ToString() => Value;
}
