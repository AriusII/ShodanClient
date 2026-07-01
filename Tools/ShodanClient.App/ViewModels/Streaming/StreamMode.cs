namespace ShodanClient.App.ViewModels.Streaming;

/// <summary>Which of the 8 <c>IStreamService</c> banner feeds the Streaming module is consuming.</summary>
public enum StreamMode
{
	/// <summary>The firehose of every banner Shodan collects (<c>StreamAllBannersAsync</c>).</summary>
	AllBanners,

	/// <summary>Banners collected for a set of Autonomous System Numbers (<c>StreamByAsnAsync</c>).</summary>
	Asn,

	/// <summary>Banners collected for a set of countries (<c>StreamByCountriesAsync</c>).</summary>
	Countries,

	/// <summary>Banners collected on a set of ports (<c>StreamByPortsAsync</c>).</summary>
	Ports,

	/// <summary>Banners affected by a set of CVEs (<c>StreamByVulnerabilitiesAsync</c>).</summary>
	Vulnerabilities,

	/// <summary>Banners matching a raw Shodan search query (<c>StreamByQueryAsync</c>).</summary>
	Query,

	/// <summary>Banners across every one of the caller's network alerts (<c>StreamAlertsAsync</c>).</summary>
	Alerts,

	/// <summary>Banners for a single network alert (<c>StreamAlertAsync</c>).</summary>
	Alert
}
