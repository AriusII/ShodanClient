using System.Diagnostics.CodeAnalysis;

namespace ShodanClient.Domain.Common;

/// <summary>
///     A Common Platform Enumeration (CPE) string, e.g. <c>cpe:2.3:a:apache:http_server:2.4.1</c>
///     (CPE 2.3) or <c>cpe:/a:apache:http_server:2.4.1</c> (CPE 2.2). Immutable value object.
/// </summary>
public readonly record struct Cpe
{
	private readonly string? _value;

	/// <summary>Creates a CPE value object.</summary>
	/// <exception cref="ArgumentException">The value is null or whitespace.</exception>
	public Cpe(string value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(value);
		_value = value.Trim();
	}

	/// <summary>The CPE string, or an empty string for a default instance.</summary>
	public string Value => _value ?? string.Empty;

	/// <summary>Whether this instance holds a value (i.e. is not <c>default</c>).</summary>
	public bool HasValue => _value is not null;

	/// <summary>Whether this is a CPE 2.3 formatted string (<c>cpe:2.3:...</c>).</summary>
	public bool IsVersion23 => Value.StartsWith("cpe:2.3:", StringComparison.OrdinalIgnoreCase);

	/// <summary>Parses a CPE string.</summary>
	public static Cpe Parse(string value) => new(value);

	/// <summary>Attempts to parse a CPE string without throwing.</summary>
	public static bool TryParse([NotNullWhen(true)] string? value, out Cpe cpe)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			cpe = default;
			return false;
		}

		cpe = new Cpe(value);
		return true;
	}

	/// <inheritdoc />
	public override string ToString() => Value;
}
