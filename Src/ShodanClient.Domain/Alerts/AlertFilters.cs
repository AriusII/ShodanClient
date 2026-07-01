namespace ShodanClient.Domain.Alerts;

/// <summary>The criteria an alert watches for, as reported on <see cref="Alert.Filters" />.</summary>
public sealed record AlertFilters
{
	/// <summary>The IPs or network ranges (CIDR notation) the alert monitors (<c>ip</c>).</summary>
	public IReadOnlyList<string> Ip { get; init; } = [];
}
