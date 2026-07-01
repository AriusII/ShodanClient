using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using ShodanClient.App.Services.AccountSession;
using ShodanClient.App.Services.Navigation;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.Profiles;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.App.Session;
using ShodanClient.App.ViewModels.Setup;

namespace ShodanClient.App.ViewModels.Shell;

/// <summary>
///     The application shell: the navigation sidebar, the currently displayed module, and the
///     always-visible account status/notification surfaces. Also owns the one-time boot sequence
///     that reattaches the previously-active profile and gates navigation on
///     <see cref="IShodanClientAccessor.IsConfigured" />.
/// </summary>
public sealed partial class ShellViewModel : ViewModelBase
{
	private readonly IShodanClientAccessor _accessor;
	private readonly INavigationService _navigation;
	private readonly IPlanCapabilities _planCapabilities;

	/// <summary>Creates the shell view model and kicks off the boot sequence.</summary>
	public ShellViewModel(
		INavigationService navigation,
		IAccountSessionService account,
		INotificationService notifications,
		IShodanClientAccessor accessor,
		IProfileService profileService,
		IPlanCapabilities planCapabilities,
		AccountStatusWidgetViewModel accountWidget)
	{
		_navigation = navigation;
		_accessor = accessor;
		_planCapabilities = planCapabilities;
		Account = account;
		Notifications = notifications;
		AccountWidget = accountWidget;

		_navigation.PropertyChanged += OnNavigationPropertyChanged;
		_accessor.Changed += OnAccessorChanged;
		_planCapabilities.Changed += OnPlanCapabilitiesChanged;
		Notifications.Items.CollectionChanged += OnNotificationsChanged;

		BuildItems();
		RefreshNavState();

		_ = BootstrapAsync(profileService);
	}

	/// <summary>
	///     Whether the shell is still attempting to reattach a previously-saved profile at startup.
	///     While <see langword="true" />, the shell shows a boot splash instead of blank content.
	/// </summary>
	[ObservableProperty]
	public partial bool IsBootstrapping { get; set; } = true;

	[ObservableProperty] public partial NavigationItemViewModel? SelectedItem { get; set; }

	/// <summary>The sidebar's main navigation entries (Explore/Manage/Enterprise groups).</summary>
	public ObservableCollection<NavigationItemViewModel> Items { get; } = [];

	/// <summary>The sidebar's pinned footer entries (Diagnostics, Settings), above <c>AccountStatusWidget</c>.</summary>
	public ObservableCollection<NavigationItemViewModel> FooterItems { get; } = [];

	/// <summary>The account/plan tracking service, exposed for anything that only needs the raw snapshot.</summary>
	public IAccountSessionService Account { get; }

	/// <summary>The shell footer's account/profile-switching widget, bound to by <c>AccountStatusWidget</c>.</summary>
	public AccountStatusWidgetViewModel AccountWidget { get; }

	/// <summary>The notification service, bound to by the shell's toast/banner host.</summary>
	public INotificationService Notifications { get; }

	/// <summary>The view model currently hosted by the shell's content area.</summary>
	public ViewModelBase? CurrentViewModel => _navigation.CurrentViewModel;

	/// <summary>Whether the first-run onboarding gate is the current module, so the shell can hide its nav pane.</summary>
	public bool IsSetupActive => CurrentViewModel is SetupViewModel;

	/// <summary>The most recent notification's message, shown in the status bar, if any.</summary>
	public string? LatestNotificationMessage => Notifications.Items.Count > 0 ? Notifications.Items[0].Message : null;

	partial void OnSelectedItemChanged(NavigationItemViewModel? value)
	{
		if (value is not null)
		{
			_navigation.NavigateTo(value.ModuleKey);
		}
	}

	/// <summary>Dismisses a shown toast/banner.</summary>
	[RelayCommand]
	private void DismissNotification(NotificationItem item) => Notifications.Dismiss(item);

	/// <summary>Invokes a toast/banner's follow-up action (e.g. "Retry"), if any.</summary>
	[RelayCommand]
	private static void InvokeNotificationAction(NotificationItem item) => item.Action?.Invoke();

	private async Task BootstrapAsync(IProfileService profileService)
	{
		IsBootstrapping = true;
		try
		{
			await profileService.InitializeAsync().ConfigureAwait(true);
		}
		finally
		{
			_navigation.NavigateTo(_accessor.IsConfigured ? ModuleKey.Dashboard : ModuleKey.Setup);
			IsBootstrapping = false;
		}
	}

	private void OnAccessorChanged()
	{
		var isConfigured = _accessor.IsConfigured;
		RefreshNavState();

		// Mirrors the boot sequence's own gating: whenever the client goes from configured to
		// unconfigured after startup (the last profile was removed, or a key was otherwise cleared),
		// fall back to the Setup gate. Successful attaches (adding/switching a profile) deliberately
		// do NOT force navigation, so switching profiles from elsewhere in the app doesn't yank the
		// user away from whatever module they were using.
		if (!isConfigured && !IsBootstrapping)
		{
			_navigation.NavigateTo(ModuleKey.Setup);
		}
	}

	private void OnPlanCapabilitiesChanged() => RefreshNavState();

	private void OnNotificationsChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
		OnPropertyChanged(nameof(LatestNotificationMessage));

	private void OnNavigationPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(INavigationService.CurrentViewModel))
		{
			return;
		}

		OnPropertyChanged(nameof(CurrentViewModel));
		OnPropertyChanged(nameof(IsSetupActive));
	}

	private void RefreshNavState()
	{
		var isConfigured = _accessor.IsConfigured;

		foreach (var item in Items)
		{
			item.IsEnabled = isConfigured;
			item.DisabledReason = null;
		}

		foreach (var item in FooterItems)
		{
			item.IsEnabled = isConfigured || item.ModuleKey is ModuleKey.Settings;
		}

		if (!isConfigured)
		{
			return;
		}

		ApplyPlanCapability(ModuleKey.Organization, _planCapabilities.OrganizationAccessDenied,
			"Your plan does not include Organization access.");
		ApplyPlanCapability(ModuleKey.BulkData, _planCapabilities.BulkDataAccessDenied,
			"Your plan does not include Bulk Data access.");
	}

	private void ApplyPlanCapability(ModuleKey moduleKey, bool accessDenied, string reason)
	{
		if (!accessDenied)
		{
			return;
		}

		var item = Items.FirstOrDefault(i => i.ModuleKey == moduleKey);
		if (item is null)
		{
			return;
		}

		item.IsEnabled = false;
		item.DisabledReason = reason;
	}

	private void BuildItems()
	{
		// Dashboard first, so it is the natural default selection once configured.
		Items.Add(new NavigationItemViewModel("Dashboard", FASymbol.Home, ModuleKey.Dashboard));

		// Explore
		Items.Add(new NavigationItemViewModel("Search", FASymbol.Find, ModuleKey.Search));
		Items.Add(new NavigationItemViewModel("Host Lookup", FASymbol.Globe, ModuleKey.HostLookup));
		Items.Add(new NavigationItemViewModel("Streaming", FASymbol.Play, ModuleKey.Streaming));
		Items.Add(new NavigationItemViewModel("Trends", FASymbol.ShowResults, ModuleKey.Trends));
		Items.Add(new NavigationItemViewModel("Exploits", FASymbol.Important, ModuleKey.Exploits));
		Items.Add(new NavigationItemViewModel("DNS Tools", FASymbol.Character, ModuleKey.Dns));

		// Manage
		Items.Add(new NavigationItemViewModel("Alerts", FASymbol.Alert, ModuleKey.Alerts));
		Items.Add(new NavigationItemViewModel("Notifiers", FASymbol.Mail, ModuleKey.Notifiers));
		Items.Add(new NavigationItemViewModel("Scans", FASymbol.Scan, ModuleKey.Scans));
		Items.Add(new NavigationItemViewModel("Directory", FASymbol.Library, ModuleKey.Directory));

		// Enterprise
		Items.Add(new NavigationItemViewModel("Organization", FASymbol.People, ModuleKey.Organization));
		Items.Add(new NavigationItemViewModel("Bulk Data", FASymbol.Download, ModuleKey.BulkData));

		// Footer (always pinned above the account status widget).
		FooterItems.Add(new NavigationItemViewModel("Diagnostics", FASymbol.Repair, ModuleKey.Diagnostics));
		FooterItems.Add(new NavigationItemViewModel("Settings", FASymbol.Settings, ModuleKey.Settings));
	}
}
