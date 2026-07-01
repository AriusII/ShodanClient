namespace ShodanClient.Application.Configuration;

/// <summary>
///     Per-surface request timeouts. The streaming surface is intentionally excluded — its
///     connection is long-lived and always uses <see cref="System.Threading.Timeout.InfiniteTimeSpan" />.
/// </summary>
/// <remarks>
///     These properties are deliberately NOT annotated with <c>[Range(typeof(TimeSpan), ...)]</c>:
///     that constructor overload is annotated <c>[RequiresUnreferencedCode]</c> (it needs a
///     <see cref="System.ComponentModel.TypeConverter" /> to parse the bounds), which would make this
///     AOT/trim-safe library emit an IL2026 trim warning for a Range check on a type nobody misconfigures
///     with negative or absurd values in practice.
/// </remarks>
public sealed class ShodanTimeoutOptions
{
	/// <summary>Timeout for core REST API calls (default 30s).</summary>
	public TimeSpan Rest { get; set; } = TimeSpan.FromSeconds(30);

	/// <summary>Timeout for Trends API calls (default 30s).</summary>
	public TimeSpan Trends { get; set; } = TimeSpan.FromSeconds(30);

	/// <summary>Timeout for Exploits API calls (default 30s).</summary>
	public TimeSpan Exploits { get; set; } = TimeSpan.FromSeconds(30);

	/// <summary>Timeout for InternetDB API calls (default 15s).</summary>
	public TimeSpan InternetDb { get; set; } = TimeSpan.FromSeconds(15);
}
