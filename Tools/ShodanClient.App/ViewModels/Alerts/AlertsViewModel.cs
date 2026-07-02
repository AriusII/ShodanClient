using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.Navigation;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.Application.Exceptions;
using ShodanClient.Domain.Alerts;
using ShodanClient.Domain.Notifiers;

namespace ShodanClient.App.ViewModels.Alerts;

/// <summary>
///     Network alert CRUD, trigger management and notifier attach/detach. Cross-references the
///     Notifiers module's data by calling <c>INotifierService.ListAsync</c> directly (rather than
///     sharing the Notifiers module's view model instance) whenever the attach picker is opened.
/// </summary>
public sealed partial class AlertsViewModel : ModuleViewModelBase
{
	private readonly INavigationService _navigation;
	private readonly INotificationService _notifications;

	/// <summary>
	///     Set while <see cref="ReplaceAlert" /> reassigns <see cref="SelectedAlert" /> to a freshly
	///     refreshed instance of the *same* alert (after toggling a trigger, or attaching/detaching a
	///     notifier), so <see cref="OnSelectedAlertChanged" /> can tell that apart from the user
	///     actually picking a different alert and avoid clobbering in-progress edits.
	/// </summary>
	private bool _isRefreshingSelectedAlertInPlace;

	private bool _triggerCatalogLoaded;

	/// <summary>Creates the Alerts module view model.</summary>
	public AlertsViewModel(INotificationService notifications, IShodanClientAccessor accessor,
		INavigationService navigation)
		: base(notifications)
	{
		Accessor = accessor;
		_navigation = navigation;
		_notifications = notifications;
		Title = "Alerts";

		_ = LoadCommand.ExecuteAsync(null);
	}

	[ObservableProperty] public partial bool IsDeleteConfirmationPending { get; private set; }

	[ObservableProperty] public partial bool IsNotifierPickerOpen { get; private set; }

	[ObservableProperty] public partial string NewAlertExpiresSecondsInput { get; set; } = string.Empty;

	[ObservableProperty] public partial string NewAlertName { get; set; } = string.Empty;

	[ObservableProperty] public partial string NewAlertNetworksInput { get; set; } = string.Empty;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(AttachNotifierCommand))]
	public partial Notifier? NotifierToAttach { get; set; }

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
	[NotifyCanExecuteChangedFor(nameof(UpdateNetworksCommand))]
	[NotifyCanExecuteChangedFor(nameof(OpenNotifierPickerCommand))]
	[NotifyCanExecuteChangedFor(nameof(AttachNotifierCommand))]
	[NotifyCanExecuteChangedFor(nameof(StreamThisAlertCommand))]
	public partial Alert? SelectedAlert { get; set; }

	[ObservableProperty] public partial string UpdateNetworksInput { get; set; } = string.Empty;

	/// <summary>The active Shodan client accessor.</summary>
	public IShodanClientAccessor Accessor { get; }

	/// <summary>Every alert configured on the account.</summary>
	public ObservableCollection<Alert> Alerts { get; } = [];

	/// <summary>The full catalogue of triggers Shodan supports, loaded once.</summary>
	public ObservableCollection<TriggerDefinition> TriggerCatalog { get; } = [];

	/// <summary>The trigger rows rendered for <see cref="SelectedAlert" />.</summary>
	public ObservableCollection<TriggerToggleViewModel> TriggerToggles { get; } = [];

	/// <summary>Notifiers available to attach to <see cref="SelectedAlert" />, loaded on demand.</summary>
	public ObservableCollection<Notifier> AttachableNotifiers { get; } = [];

	partial void OnSelectedAlertChanged(Alert? value)
	{
		if (_isRefreshingSelectedAlertInPlace)
		{
			// Just a background refresh of the alert the user already has open (a trigger toggle,
			// a notifier attach/detach, a networks save) — only the trigger rows' enabled state
			// needs to catch up. Leave any in-progress "Monitored networks" edit and the notifier
			// picker's open/selected state alone.
			RefreshTriggerToggleStates();
			return;
		}

		UpdateNetworksInput = value is null ? string.Empty : string.Join(Environment.NewLine, value.Filters.Ip);
		IsNotifierPickerOpen = false;
		NotifierToAttach = null;
		IsDeleteConfirmationPending = false;
		RebuildTriggerToggles();
	}

	/// <summary>Reloads the alert list and (once) the trigger catalogue.</summary>
	[RelayCommand]
	private Task LoadAsync() => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client)
		{
			return;
		}

		var previousSelectedId = SelectedAlert?.Id;

		var alerts = await client.Alerts.ListAsync(ct).ConfigureAwait(true);
		Alerts.Clear();
		foreach (var alert in alerts)
		{
			Alerts.Add(alert);
		}

		// ObservableCollection.Clear() raises a Reset notification that would otherwise deselect
		// the ListBox's SelectedItem; restore the previous selection by id so refreshing doesn't
		// silently collapse the detail pane.
		SelectedAlert = previousSelectedId is null ? null : Alerts.FirstOrDefault(a => a.Id == previousSelectedId);

		if (!_triggerCatalogLoaded)
		{
			var triggers = await client.Alerts.GetTriggersAsync(ct).ConfigureAwait(true);
			TriggerCatalog.Clear();
			foreach (var trigger in triggers)
			{
				TriggerCatalog.Add(trigger);
			}

			_triggerCatalogLoaded = true;
			RebuildTriggerToggles();
		}
	});

	/// <summary>Creates a new alert from <see cref="NewAlertName" />/<see cref="NewAlertNetworksInput" />.</summary>
	[RelayCommand]
	private Task CreateAsync() => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client)
		{
			return;
		}

		var name = NewAlertName.Trim();
		if (name.Length == 0)
		{
			throw new ShodanRequestValidationException("Enter a name for the alert.", nameof(NewAlertName));
		}

		var ips = ParseNetworks(NewAlertNetworksInput);
		if (ips.Length == 0)
		{
			throw new ShodanRequestValidationException(
				"Enter at least one IP or CIDR range to monitor.",
				nameof(NewAlertNetworksInput));
		}

		var expiresSeconds = 0;
		if (!string.IsNullOrWhiteSpace(NewAlertExpiresSecondsInput) &&
			!int.TryParse(NewAlertExpiresSecondsInput, out expiresSeconds))
		{
			throw new ShodanRequestValidationException(
				"Expiration must be a whole number of seconds.",
				nameof(NewAlertExpiresSecondsInput));
		}

		var created = await client.Alerts.CreateAsync(name, ips, expiresSeconds, ct).ConfigureAwait(true);
		Alerts.Add(created);
		SelectedAlert = created;
		NewAlertName = string.Empty;
		NewAlertNetworksInput = string.Empty;
		NewAlertExpiresSecondsInput = string.Empty;
		_notifications.Success($"Alert \"{created.Name}\" created.");
	});

	private bool HasSelectedAlert() => SelectedAlert is not null;

	/// <summary>
	///     Deletes <see cref="SelectedAlert" />. The first click only arms a confirmation (a warning
	///     toast naming the alert); the second click actually deletes it, matching the Scans module's
	///     confirm-before-destructive-action convention.
	/// </summary>
	[RelayCommand(CanExecute = nameof(HasSelectedAlert))]
	private Task DeleteAsync()
	{
		if (SelectedAlert is not { } alert)
		{
			return Task.CompletedTask;
		}

		if (!IsDeleteConfirmationPending)
		{
			IsDeleteConfirmationPending = true;
			_notifications.Warning(
				$"Click \"Delete alert\" again to permanently delete \"{alert.Name}\" and its trigger/notifier configuration.",
				"Confirm delete");
			return Task.CompletedTask;
		}

		IsDeleteConfirmationPending = false;
		return RunAsync(async ct =>
		{
			if (Accessor.Client is not { } client || SelectedAlert is not { } current)
			{
				return;
			}

			var deleted = await client.Alerts.DeleteAsync(current.Id, ct).ConfigureAwait(true);
			if (deleted)
			{
				Alerts.Remove(current);
				SelectedAlert = null;
				_notifications.Success($"Alert \"{current.Name}\" deleted.");
			}
			else
			{
				_notifications.Warning($"Shodan did not delete \"{current.Name}\".");
			}
		});
	}

	/// <summary>Replaces <see cref="SelectedAlert" />'s monitored networks with <see cref="UpdateNetworksInput" />.</summary>
	[RelayCommand(CanExecute = nameof(HasSelectedAlert))]
	private Task UpdateNetworksAsync() => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client || SelectedAlert is not { } alert)
		{
			return;
		}

		var ips = ParseNetworks(UpdateNetworksInput);
		if (ips.Length == 0)
		{
			throw new ShodanRequestValidationException(
				"Enter at least one IP or CIDR range to monitor.",
				nameof(UpdateNetworksInput));
		}

		var updated = await client.Alerts.UpdateNetworksAsync(alert.Id, ips, ct).ConfigureAwait(true);
		ReplaceAlert(updated);
		_notifications.Success("Monitored networks updated.");
	});

	/// <summary>
	///     Loads the notifiers available to attach (excluding ones already attached to
	///     <see cref="SelectedAlert" />) and opens the attach picker.
	/// </summary>
	[RelayCommand(CanExecute = nameof(HasSelectedAlert))]
	private Task OpenNotifierPickerAsync() => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client || SelectedAlert is not { } alert)
		{
			return;
		}

		var attachedIds = alert.Notifiers.Select(n => n.Id).ToHashSet(StringComparer.Ordinal);
		var notifiers = await client.Notifiers.ListAsync(ct).ConfigureAwait(true);
		AttachableNotifiers.Clear();
		foreach (var notifier in notifiers.Where(n => !attachedIds.Contains(n.Id)))
		{
			AttachableNotifiers.Add(notifier);
		}

		IsNotifierPickerOpen = true;
	});

	/// <summary>Closes the notifier attach picker without attaching anything.</summary>
	[RelayCommand]
	private void CancelNotifierPicker()
	{
		IsNotifierPickerOpen = false;
		NotifierToAttach = null;
	}

	private bool CanAttachNotifier() => SelectedAlert is not null && NotifierToAttach is not null;

	/// <summary>Attaches <see cref="NotifierToAttach" /> to <see cref="SelectedAlert" />.</summary>
	[RelayCommand(CanExecute = nameof(CanAttachNotifier))]
	private Task AttachNotifierAsync() => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client || SelectedAlert is not { } alert || NotifierToAttach is not { } notifier)
		{
			return;
		}

		var attached = await client.Alerts.AddNotifierAsync(alert.Id, notifier.Id, ct).ConfigureAwait(true);
		if (!attached)
		{
			_notifications.Warning(
				"Shodan did not attach the notifier; it may already be attached or no longer exist.");
			return;
		}

		await RefreshAlertAsync(client, alert.Id, ct).ConfigureAwait(true);
		IsNotifierPickerOpen = false;
		NotifierToAttach = null;
		_notifications.Success("Notifier attached.");
	});

	/// <summary>Detaches a notifier from <see cref="SelectedAlert" />.</summary>
	[RelayCommand]
	private Task DetachNotifierAsync(AlertNotifierRef notifierRef) => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client || SelectedAlert is not { } alert)
		{
			return;
		}

		var removed = await client.Alerts.RemoveNotifierAsync(alert.Id, notifierRef.Id, ct).ConfigureAwait(true);
		if (!removed)
		{
			_notifications.Warning("Shodan did not detach the notifier.");
			return;
		}

		await RefreshAlertAsync(client, alert.Id, ct).ConfigureAwait(true);
		_notifications.Success("Notifier detached.");
	});

	/// <summary>Navigates to the Streaming module so the user can watch this alert's matches live.</summary>
	[RelayCommand(CanExecute = nameof(HasSelectedAlert))]
	private void StreamThisAlert() => _navigation.NavigateTo(ModuleKey.Streaming, SelectedAlert?.Id);

	private Task ToggleTriggerAsync(TriggerToggleViewModel row) => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client || SelectedAlert is not { } alert)
		{
			return;
		}

		var wasEnabled = row.IsEnabled;
		var succeeded = wasEnabled
			? await client.Alerts.DisableTriggerAsync(alert.Id, row.Name, ct).ConfigureAwait(true)
			: await client.Alerts.EnableTriggerAsync(alert.Id, row.Name, ct).ConfigureAwait(true);

		if (!succeeded)
		{
			_notifications.Warning(
				$"Shodan did not {(wasEnabled ? "disable" : "enable")} the \"{row.Name}\" trigger.");
			return;
		}

		await RefreshAlertAsync(client, alert.Id, ct).ConfigureAwait(true);
	});

	private Task IgnoreTriggerServiceAsync(TriggerToggleViewModel row) => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client || SelectedAlert is not { } alert)
		{
			return;
		}

		var service = row.ServiceInput.Trim();
		var ignored = await client.Alerts.IgnoreTriggerServiceAsync(alert.Id, row.Name, service, ct)
			.ConfigureAwait(true);
		if (!ignored)
		{
			_notifications.Warning($"Shodan did not whitelist {service} for the \"{row.Name}\" trigger.");
			return;
		}

		row.ServiceInput = string.Empty;
		_notifications.Success($"{service} whitelisted for the \"{row.Name}\" trigger.");
	});

	private Task UnignoreTriggerServiceAsync(TriggerToggleViewModel row) => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client || SelectedAlert is not { } alert)
		{
			return;
		}

		var service = row.ServiceInput.Trim();
		var unignored = await client.Alerts.UnignoreTriggerServiceAsync(alert.Id, row.Name, service, ct)
			.ConfigureAwait(true);
		if (!unignored)
		{
			_notifications.Warning($"Shodan did not remove {service} from the \"{row.Name}\" trigger's whitelist.");
			return;
		}

		row.ServiceInput = string.Empty;
		_notifications.Success($"{service} removed from the \"{row.Name}\" trigger's whitelist.");
	});

	private async Task RefreshAlertAsync(IShodanClient client, string id, CancellationToken ct)
	{
		var refreshed = await client.Alerts.GetAsync(id, ct).ConfigureAwait(true);
		ReplaceAlert(refreshed);
	}

	private void ReplaceAlert(Alert updated)
	{
		for (var i = 0; i < Alerts.Count; i++)
		{
			if (Alerts[i].Id != updated.Id)
			{
				continue;
			}

			Alerts[i] = updated;
			break;
		}

		// Every caller of ReplaceAlert refreshes the alert currently open in the detail pane (after
		// toggling a trigger, or attaching/detaching a notifier), so if it's still the selected one
		// this is an in-place refresh, not the user picking a different alert.
		var isInPlaceRefresh = SelectedAlert?.Id == updated.Id;
		_isRefreshingSelectedAlertInPlace = isInPlaceRefresh;
		try
		{
			SelectedAlert = updated;
		}
		finally
		{
			_isRefreshingSelectedAlertInPlace = false;
		}
	}

	private void RebuildTriggerToggles()
	{
		TriggerToggles.Clear();
		if (SelectedAlert is not { } alert)
		{
			return;
		}

		foreach (var definition in TriggerCatalog)
		{
			var isEnabled = alert.Triggers.ContainsKey(definition.Name);
			TriggerToggles.Add(new TriggerToggleViewModel(
				definition.Name,
				definition.Description,
				isEnabled,
				ToggleTriggerAsync,
				IgnoreTriggerServiceAsync,
				UnignoreTriggerServiceAsync));
		}
	}

	/// <summary>
	///     Updates each existing <see cref="TriggerToggleViewModel" /> row's enabled state in place
	///     (used for the in-place refresh path) instead of tearing down and recreating every row,
	///     which would otherwise wipe out any "ip:port" whitelist text the user is mid-typing in a
	///     sibling row's <see cref="TriggerToggleViewModel.ServiceInput" />.
	/// </summary>
	private void RefreshTriggerToggleStates()
	{
		if (SelectedAlert is not { } alert)
		{
			return;
		}

		foreach (var row in TriggerToggles)
		{
			row.IsEnabled = alert.Triggers.ContainsKey(row.Name);
		}
	}

	private static string[] ParseNetworks(string text) =>
		text.Split([',', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
