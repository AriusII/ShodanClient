namespace ShodanClient.Domain.Search;

/// <summary>Identifies the network alert that produced a banner on an alert stream (<c>_shodan.alert</c>).</summary>
public sealed record BannerAlertReference
{
	/// <summary>The triggering alert's unique identifier (<c>_shodan.alert.id</c>).</summary>
	public string? Id { get; init; }

	/// <summary>The triggering alert's display name (<c>_shodan.alert.name</c>).</summary>
	public string? Name { get; init; }
}
