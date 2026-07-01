namespace ShodanClient.Domain.Common;

/// <summary>
///     Geographic location associated with a host or banner, as reported by Shodan's
///     <c>location</c> block. All members are optional; Shodan only populates what it knows.
/// </summary>
public sealed record GeoLocation
{
	/// <summary>City name (<c>location.city</c>).</summary>
	public string? City { get; init; }

	/// <summary>Region/state code (<c>location.region_code</c>).</summary>
	public string? RegionCode { get; init; }

	/// <summary>Two-letter ISO country code (<c>location.country_code</c>).</summary>
	public string? CountryCode { get; init; }

	/// <summary>Legacy three-letter country code (<c>location.country_code3</c>).</summary>
	public string? CountryCode3 { get; init; }

	/// <summary>Country name (<c>location.country_name</c>).</summary>
	public string? CountryName { get; init; }

	/// <summary>Postal code (<c>location.postal_code</c>, legacy).</summary>
	public string? PostalCode { get; init; }

	/// <summary>US telephone area code (<c>location.area_code</c>, legacy).</summary>
	public int? AreaCode { get; init; }

	/// <summary>US DMA code (<c>location.dma_code</c>, legacy).</summary>
	public int? DmaCode { get; init; }

	/// <summary>Latitude (<c>location.latitude</c>).</summary>
	public double? Latitude { get; init; }

	/// <summary>Longitude (<c>location.longitude</c>).</summary>
	public double? Longitude { get; init; }
}
