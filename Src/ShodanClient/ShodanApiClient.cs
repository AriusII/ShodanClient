using ShodanClient.Application.Abstractions.Services;

namespace ShodanClient;

/// <summary>
///     Default <see cref="IShodanClient" /> implementation: a thin aggregator that exposes the injected
///     service sub-clients. All behavior lives in the services; this type only groups them.
/// </summary>
internal sealed class ShodanApiClient(
	IHostService hosts,
	ISearchService search,
	IScanService scans,
	IAlertService alerts,
	INotifierService notifiers,
	IQueryDirectoryService directory,
	IBulkDataService bulkData,
	IOrganizationService organization,
	IAccountService account,
	IDnsService dns,
	IUtilityService tools,
	IApiStatusService apiInfo,
	IInternetDbService internetDb,
	ITrendsService trends,
	IExploitService exploits,
	IStreamService stream) : IShodanClient
{
	public IHostService Hosts => hosts;

	public ISearchService Search => search;

	public IScanService Scans => scans;

	public IAlertService Alerts => alerts;

	public INotifierService Notifiers => notifiers;

	public IQueryDirectoryService Directory => directory;

	public IBulkDataService BulkData => bulkData;

	public IOrganizationService Organization => organization;

	public IAccountService Account => account;

	public IDnsService Dns => dns;

	public IUtilityService Tools => tools;

	public IApiStatusService ApiInfo => apiInfo;

	public IInternetDbService InternetDb => internetDb;

	public ITrendsService Trends => trends;

	public IExploitService Exploits => exploits;

	public IStreamService Stream => stream;
}
