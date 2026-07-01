using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShodanClient.App.Services.AccountSession;
using ShodanClient.App.Services.Credentials;
using ShodanClient.App.Services.Navigation;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.Profiles;
using ShodanClient.App.Services.Settings;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.App.Session;
using ShodanClient.App.ViewModels.Alerts;
using ShodanClient.App.ViewModels.BulkData;
using ShodanClient.App.ViewModels.Dashboard;
using ShodanClient.App.ViewModels.Diagnostics;
using ShodanClient.App.ViewModels.Dns;
using ShodanClient.App.ViewModels.Exploits;
using ShodanClient.App.ViewModels.HostLookup;
using ShodanClient.App.ViewModels.Notifiers;
using ShodanClient.App.ViewModels.Organization;
using ShodanClient.App.ViewModels.QueryDirectory;
using ShodanClient.App.ViewModels.Scans;
using ShodanClient.App.ViewModels.Search;
using ShodanClient.App.ViewModels.Settings;
using ShodanClient.App.ViewModels.Setup;
using ShodanClient.App.ViewModels.Shell;
using ShodanClient.App.ViewModels.Streaming;
using ShodanClient.App.ViewModels.Trends;

namespace ShodanClient.App.Composition;

/// <summary>
///     Builds the app's main DI container. Deliberately does NOT register <see cref="IShodanClient" />
///     or call <c>AddShodanClient</c> — that only happens inside <see cref="ShodanClientAccessor" />'s
///     own inner host, so the app can start without a key and rotate it at runtime.
/// </summary>
public static class CompositionRoot
{
	/// <summary>Builds (but does not start) the app's host.</summary>
	public static IHost Build()
	{
		var builder = Host.CreateApplicationBuilder();
		builder.Logging.ClearProviders();

		var services = builder.Services;

		// App-local singleton services.
		services.AddSingleton<ICredentialStore, DpapiCredentialStore>();
		services.AddSingleton<ISettingsService, LocalSettingsService>();
		services.AddSingleton<IShodanClientAccessor, ShodanClientAccessor>();
		services.AddSingleton<IAccountSessionService, AccountSessionService>();
		services.AddSingleton<INavigationService, NavigationService>();
		services.AddSingleton<INotificationService, NotificationService>();
		services.AddSingleton<IProfileService, ProfileService>();
		services.AddSingleton<IPlanCapabilities, PlanCapabilities>();

		// Shell.
		services.AddSingleton<ShellViewModel>();
		services.AddSingleton<AccountStatusWidgetViewModel>();

		// Modules — resolved lazily by NavigationService on first navigation, never eagerly here.
		services.AddSingleton<SetupViewModel>();
		services.AddSingleton<DashboardViewModel>();
		services.AddSingleton<SearchModuleViewModel>();
		services.AddSingleton<HostLookupViewModel>();
		services.AddSingleton<StreamingViewModel>();
		services.AddSingleton<AlertsViewModel>();
		services.AddSingleton<NotifiersViewModel>();
		services.AddSingleton<ScansViewModel>();
		services.AddSingleton<QueryDirectoryViewModel>();
		services.AddSingleton<DnsViewModel>();
		services.AddSingleton<TrendsViewModel>();
		services.AddSingleton<ExploitsViewModel>();
		services.AddSingleton<OrganizationViewModel>();
		services.AddSingleton<BulkDataViewModel>();
		services.AddSingleton<DiagnosticsViewModel>();
		services.AddSingleton<SettingsViewModel>();

		return builder.Build();
	}
}
