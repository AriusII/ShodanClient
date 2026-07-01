using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ShodanClient.App.ViewModels;
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

namespace ShodanClient.App.Services.Navigation;

/// <summary>
///     Default <see cref="INavigationService" />: resolves module view models lazily from
///     <see cref="IServiceProvider" /> and caches the resolved instance per <see cref="ModuleKey" /> so
///     module state (an in-progress filter, an active stream, …) survives switching tabs.
/// </summary>
/// <remarks>Creates the navigation service.</remarks>
public sealed partial class NavigationService(IServiceProvider serviceProvider) : ObservableObject, INavigationService
{
	private static readonly Dictionary<ModuleKey, Type> ModuleTypes = new()
	{
		[ModuleKey.Dashboard] = typeof(DashboardViewModel),
		[ModuleKey.Search] = typeof(SearchModuleViewModel),
		[ModuleKey.HostLookup] = typeof(HostLookupViewModel),
		[ModuleKey.Streaming] = typeof(StreamingViewModel),
		[ModuleKey.Alerts] = typeof(AlertsViewModel),
		[ModuleKey.Notifiers] = typeof(NotifiersViewModel),
		[ModuleKey.Scans] = typeof(ScansViewModel),
		[ModuleKey.Directory] = typeof(QueryDirectoryViewModel),
		[ModuleKey.Dns] = typeof(DnsViewModel),
		[ModuleKey.Trends] = typeof(TrendsViewModel),
		[ModuleKey.Exploits] = typeof(ExploitsViewModel),
		[ModuleKey.Organization] = typeof(OrganizationViewModel),
		[ModuleKey.BulkData] = typeof(BulkDataViewModel),
		[ModuleKey.Diagnostics] = typeof(DiagnosticsViewModel),
		[ModuleKey.Settings] = typeof(SettingsViewModel),
		[ModuleKey.Setup] = typeof(SetupViewModel)
	};

	private readonly Dictionary<ModuleKey, ViewModelBase> _cache = [];

	[ObservableProperty] public partial ViewModelBase? CurrentViewModel { get; private set; }

	/// <inheritdoc />
	public void NavigateTo(ModuleKey key, object? parameter = null)
	{
		if (!_cache.TryGetValue(key, out var viewModel))
		{
			if (!ModuleTypes.TryGetValue(key, out var viewModelType))
			{
				throw new ArgumentOutOfRangeException(nameof(key), key, "No view model is registered for this module.");
			}

			viewModel = (ViewModelBase)serviceProvider.GetRequiredService(viewModelType);
			_cache[key] = viewModel;
		}

		if (viewModel is INavigationAwareDispatch aware)
		{
			aware.OnNavigatedTo(parameter);
		}

		CurrentViewModel = viewModel;
	}
}
