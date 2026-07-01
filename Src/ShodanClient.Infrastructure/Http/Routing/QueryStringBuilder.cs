using System.Globalization;
using System.Runtime.CompilerServices;

namespace ShodanClient.Infrastructure.Http.Routing;

/// <summary>
///     Allocation-conscious query-string composer. Backed by a pooled
///     <see cref="DefaultInterpolatedStringHandler" />, it appends URL-encoded <c>?a=b&amp;c=d</c>
///     pairs and produces the final string once. Values are escaped with
///     <see cref="Uri.EscapeDataString(string)" />. Intended for single-use per route.
/// </summary>
internal ref struct QueryStringBuilder
{
	private DefaultInterpolatedStringHandler _handler;
	private bool _hasAny;

	/// <summary>Creates a builder. <paramref name="capacityHint" /> pre-sizes the buffer.</summary>
	public QueryStringBuilder(int capacityHint = 32)
	{
		_handler = new DefaultInterpolatedStringHandler(capacityHint, 0, CultureInfo.InvariantCulture);
		_hasAny = false;
	}

	/// <summary>Appends <c>name=value</c> (value URL-encoded).</summary>
	public void Add(string name, string value)
	{
		_handler.AppendLiteral(_hasAny ? "&" : "?");
		_handler.AppendLiteral(name);
		_handler.AppendLiteral("=");
		_handler.AppendFormatted(Uri.EscapeDataString(value));
		_hasAny = true;
	}

	/// <summary>Appends an integer parameter using the invariant culture.</summary>
	public void Add(string name, int value) => Add(name, value.ToString(CultureInfo.InvariantCulture));

	/// <summary>Appends <c>name=true</c> only when <paramref name="value" /> is <see langword="true" />.</summary>
	public void AddFlag(string name, bool value)
	{
		if (value)
		{
			Add(name, "true");
		}
	}

	/// <summary>Appends the parameter only when <paramref name="value" /> is non-empty.</summary>
	public void AddIfPresent(string name, string? value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			Add(name, value);
		}
	}

	/// <summary>
	///     Returns the composed query string (with leading <c>?</c>) or an empty string if no
	///     parameters were added, and releases the pooled buffer. Call exactly once.
	/// </summary>
	public string Build()
	{
		var result = _handler.ToStringAndClear();
		return _hasAny ? result : string.Empty;
	}
}
