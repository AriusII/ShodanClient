using Microsoft.Extensions.DependencyInjection;
using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Infrastructure.Account;
using ShodanClient.Infrastructure.Alerts;
using ShodanClient.Infrastructure.ApiStatus;
using ShodanClient.Infrastructure.BulkData;
using ShodanClient.Infrastructure.Directory;
using ShodanClient.Infrastructure.Dns;
using ShodanClient.Infrastructure.Exploits;
using ShodanClient.Infrastructure.InternetDb;
using ShodanClient.Infrastructure.Notifiers;
using ShodanClient.Infrastructure.Organization;
using ShodanClient.Infrastructure.Scanning;
using ShodanClient.Infrastructure.Search;
using ShodanClient.Infrastructure.Streaming;
using ShodanClient.Infrastructure.Trends;
using ShodanClient.Infrastructure.Utility;

namespace ShodanClient.Infrastructure.DependencyInjection;

/// <summary>
///     Wires up the Infrastructure layer: the per-surface typed HTTP clients plus every repository and
///     service implementation. Called by the public <c>AddShodanClient</c> facade registration.
/// </summary>
internal static class InfrastructureServiceCollectionExtensions
{
	extension(IServiceCollection services)
	{
		public IServiceCollection AddShodanInfrastructure()
		{
			services.AddShodanChannels();
			services.AddShodanRepositories();
			services.AddShodanServices();
			return services;
		}

		private void AddShodanRepositories()
		{
			services.AddTransient<IHostRepository, HostRepository>();
			services.AddTransient<ISearchRepository, SearchRepository>();
			services.AddTransient<IAccountRepository, AccountRepository>();
			services.AddTransient<IApiStatusRepository, ApiStatusRepository>();
			services.AddTransient<IUtilityRepository, UtilityRepository>();
			services.AddTransient<IDnsRepository, DnsRepository>();
			services.AddTransient<IScanRepository, ScanRepository>();
			services.AddTransient<IDirectoryRepository, DirectoryRepository>();
			services.AddTransient<INotifierRepository, NotifierRepository>();
			services.AddTransient<IOrganizationRepository, OrganizationRepository>();
			services.AddTransient<IBulkDataRepository, BulkDataRepository>();
			services.AddTransient<IAlertRepository, AlertRepository>();
			services.AddTransient<IInternetDbRepository, InternetDbRepository>();
			services.AddTransient<ITrendsRepository, TrendsRepository>();
			services.AddTransient<IExploitRepository, ExploitRepository>();
			services.AddTransient<IStreamRepository, StreamRepository>();
		}

		private void AddShodanServices()
		{
			services.AddTransient<IHostService, HostService>();
			services.AddTransient<ISearchService, SearchService>();
			services.AddTransient<IAccountService, AccountService>();
			services.AddTransient<IApiStatusService, ApiStatusService>();
			services.AddTransient<IUtilityService, UtilityService>();
			services.AddTransient<IDnsService, DnsService>();
			services.AddTransient<IScanService, ScanService>();
			services.AddTransient<IQueryDirectoryService, DirectoryService>();
			services.AddTransient<INotifierService, NotifierService>();
			services.AddTransient<IOrganizationService, OrganizationService>();
			services.AddTransient<IBulkDataService, BulkDataService>();
			services.AddTransient<IAlertService, AlertService>();
			services.AddTransient<IInternetDbService, InternetDbService>();
			services.AddTransient<ITrendsService, TrendsService>();
			services.AddTransient<IExploitService, ExploitService>();
			services.AddTransient<IStreamService, StreamService>();
		}
	}
}
