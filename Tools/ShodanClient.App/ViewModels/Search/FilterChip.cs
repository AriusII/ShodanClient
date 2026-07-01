namespace ShodanClient.App.ViewModels.Search;

/// <summary>
///     A single <c>name:value</c> search filter chip, e.g. <c>port:443</c> or <c>org:"Example Inc"</c>,
///     as built by the Search module's query chip editor.
/// </summary>
/// <param name="Name">The filter name, e.g. <c>port</c>.</param>
/// <param name="Value">The filter value, e.g. <c>443</c>.</param>
public sealed record FilterChip(string Name, string Value)
{
	/// <summary>The chip rendered as it appears inside a composed Shodan query, e.g. <c>port:443</c>.</summary>
	public string QueryFragment => $"{Name}:{QuoteIfNeeded(Value)}";

	private static string QuoteIfNeeded(string value) =>
		value.Contains(' ', StringComparison.Ordinal) && !value.StartsWith('"')
			? $"\"{value}\""
			: value;
}
