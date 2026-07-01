using Avalonia.Controls;
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
using ShodanClient.App.ViewModels.Streaming;
using ShodanClient.App.ViewModels.Trends;
using ShodanClient.App.Views.Alerts;
using ShodanClient.App.Views.BulkData;
using ShodanClient.App.Views.Dashboard;
using ShodanClient.App.Views.Diagnostics;
using ShodanClient.App.Views.Dns;
using ShodanClient.App.Views.Exploits;
using ShodanClient.App.Views.HostLookup;
using ShodanClient.App.Views.Notifiers;
using ShodanClient.App.Views.Organization;
using ShodanClient.App.Views.QueryDirectory;
using ShodanClient.App.Views.Scans;
using ShodanClient.App.Views.Search;
using ShodanClient.App.Views.Settings;
using ShodanClient.App.Views.Setup;
using ShodanClient.App.Views.Streaming;
using ShodanClient.App.Views.Trends;

namespace ShodanClient.App.Composition;

/// <summary>
///     The closed, AOT-safe view-model-to-view map consumed by <see cref="ShodanClient.App.ViewLocator" />.
///     No reflection: every entry is a direct <c>typeof(...)</c> key and a <c>() =&gt; new View()</c>
///     factory, built once at startup.
/// </summary>
public static class ViewFactoryRegistry
{
	/// <summary>Builds the closed view-model type -&gt; view factory map.</summary>
	public static IReadOnlyDictionary<Type, Func<Control>> Build() =>
		new Dictionary<Type, Func<Control>>
		{
			[typeof(SetupViewModel)] = () => new SetupView(),
			[typeof(DashboardViewModel)] = () => new DashboardView(),
			[typeof(SearchModuleViewModel)] = () => new SearchModuleView(),
			[typeof(HostLookupViewModel)] = () => new HostLookupView(),
			[typeof(StreamingViewModel)] = () => new StreamingView(),
			[typeof(AlertsViewModel)] = () => new AlertsView(),
			[typeof(NotifiersViewModel)] = () => new NotifiersView(),
			[typeof(ScansViewModel)] = () => new ScansView(),
			[typeof(QueryDirectoryViewModel)] = () => new QueryDirectoryView(),
			[typeof(DnsViewModel)] = () => new DnsView(),
			[typeof(TrendsViewModel)] = () => new TrendsView(),
			[typeof(ExploitsViewModel)] = () => new ExploitsView(),
			[typeof(OrganizationViewModel)] = () => new OrganizationView(),
			[typeof(BulkDataViewModel)] = () => new BulkDataView(),
			[typeof(DiagnosticsViewModel)] = () => new DiagnosticsView(),
			[typeof(SettingsViewModel)] = () => new SettingsView()
		};
}
