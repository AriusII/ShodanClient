using ShodanClient.Application.Abstractions.Services;

namespace ShodanClient;

/// <summary>
///     The entry point to the Shodan API. Resolve <see cref="IShodanClient" /> from dependency injection
///     (after calling <c>AddShodanClient</c>) and access each area of the API through its sub-client
///     property.
/// </summary>
public interface IShodanClient
{
	/// <summary>Host lookups (<c>/shodan/host/{ip}</c>).</summary>
	IHostService Hosts { get; }

	/// <summary>Searching Shodan and inspecting the search grammar.</summary>
	ISearchService Search { get; }

	/// <summary>On-demand scanning and crawl status (<c>/shodan/scan</c>, <c>/shodan/scans</c>, …).</summary>
	IScanService Scans { get; }

	/// <summary>Network alerts, triggers and notifier links (<c>/shodan/alert*</c>).</summary>
	IAlertService Alerts { get; }

	/// <summary>Notification services (<c>/notifier*</c>).</summary>
	INotifierService Notifiers { get; }

	/// <summary>The directory of saved search queries (<c>/shodan/query*</c>).</summary>
	IQueryDirectoryService Directory { get; }

	/// <summary>Bulk data datasets (<c>/shodan/data*</c>, Enterprise).</summary>
	IBulkDataService BulkData { get; }

	/// <summary>Organization management (<c>/org*</c>, Enterprise).</summary>
	IOrganizationService Organization { get; }

	/// <summary>The account linked to the API key (<c>/account/profile</c>).</summary>
	IAccountService Account { get; }

	/// <summary>DNS lookups (<c>/dns/*</c>).</summary>
	IDnsService Dns { get; }

	/// <summary>Utility helpers (<c>/tools/*</c>).</summary>
	IUtilityService Tools { get; }

	/// <summary>API plan information and remaining credits (<c>/api-info</c>).</summary>
	IApiStatusService ApiInfo { get; }

	/// <summary>Fast, key-less IP summaries from InternetDB (<c>https://internetdb.shodan.io</c>).</summary>
	IInternetDbService InternetDb { get; }

	/// <summary>Historical, month-to-month search trends (<c>https://trends.shodan.io</c>).</summary>
	ITrendsService Trends { get; }

	/// <summary>Exploit and vulnerability search (<c>https://exploits.shodan.io</c>).</summary>
	IExploitService Exploits { get; }

	/// <summary>Real-time banner streaming (<c>https://stream.shodan.io</c>), consumed as <see cref="IAsyncEnumerable{T}" />.</summary>
	IStreamService Stream { get; }
}
