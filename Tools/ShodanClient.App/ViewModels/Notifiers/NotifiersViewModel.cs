using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.Domain.Notifiers;

namespace ShodanClient.App.ViewModels.Notifiers;

/// <summary>
///     Notifier CRUD with a provider-driven dynamic argument form: selecting a provider (or an
///     existing notifier to edit) renders exactly the argument fields that provider requires.
/// </summary>
public sealed partial class NotifiersViewModel : ModuleViewModelBase
{
	private readonly INotificationService _notifications;

	private bool _providersLoaded;

	/// <summary>Creates the Notifiers module view model.</summary>
	public NotifiersViewModel(INotificationService notifications, IShodanClientAccessor accessor)
		: base(notifications)
	{
		Accessor = accessor;
		_notifications = notifications;
		Title = "Notifiers";

		_ = LoadCommand.ExecuteAsync(null);
	}

	[ObservableProperty] public partial string? CreateDescription { get; set; }

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(UpdateNotifierCommand))]
	[NotifyCanExecuteChangedFor(nameof(DeleteNotifierCommand))]
	public partial Notifier? SelectedNotifier { get; set; }

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(CreateNotifierCommand))]
	public partial NotifierProvider? SelectedProvider { get; set; }

	/// <summary>The active Shodan client accessor.</summary>
	public IShodanClientAccessor Accessor { get; }

	/// <summary>The user's configured notifiers.</summary>
	public ObservableCollection<Notifier> Notifiers { get; } = [];

	/// <summary>The notification providers Shodan supports, loaded once.</summary>
	public ObservableCollection<NotifierProvider> Providers { get; } = [];

	/// <summary>The argument fields rendered for <see cref="SelectedProvider" /> when creating a notifier.</summary>
	public ObservableCollection<NotifierArgumentField> CreateArgumentFields { get; } = [];

	/// <summary>The argument fields rendered for <see cref="SelectedNotifier" /> when editing a notifier.</summary>
	public ObservableCollection<NotifierArgumentField> EditArgumentFields { get; } = [];

	partial void OnSelectedProviderChanged(NotifierProvider? value)
	{
		CreateArgumentFields.Clear();
		if (value is null)
		{
			return;
		}

		foreach (var key in value.Required)
		{
			CreateArgumentFields.Add(new NotifierArgumentField(key));
		}
	}

	partial void OnSelectedNotifierChanged(Notifier? value)
	{
		EditArgumentFields.Clear();
		if (value is null)
		{
			return;
		}

		var provider = Providers.FirstOrDefault(p => p.Name == value.Provider);
		var keys = provider is { Required.Count: > 0 } ? provider.Required : value.Args.Keys.ToList();

		foreach (var key in keys)
		{
			var existingValue = value.Args.TryGetValue(key, out var v) ? v : string.Empty;
			EditArgumentFields.Add(new NotifierArgumentField(key, existingValue));
		}
	}

	/// <summary>Reloads the notifier list and (once) the provider catalogue.</summary>
	[RelayCommand]
	private Task LoadAsync() => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client)
		{
			return;
		}

		if (!_providersLoaded)
		{
			var providers = await client.Notifiers.GetProvidersAsync(ct).ConfigureAwait(true);
			Providers.Clear();
			foreach (var provider in providers)
			{
				Providers.Add(provider);
			}

			_providersLoaded = true;
		}

		// ObservableCollection.Clear() inside ReloadNotifiersAsync raises a Reset notification that
		// would otherwise deselect the ListBox's SelectedItem; restore the previous selection by id
		// so refreshing doesn't silently collapse the "Edit selected notifier" panel.
		var previousSelectedId = SelectedNotifier?.Id;
		await ReloadNotifiersAsync(client, ct).ConfigureAwait(true);
		SelectedNotifier = previousSelectedId is null
			? null
			: Notifiers.FirstOrDefault(n => n.Id == previousSelectedId);
	});

	private bool CanCreateNotifier() => SelectedProvider is not null;

	/// <summary>Creates a notifier from <see cref="SelectedProvider" /> and <see cref="CreateArgumentFields" />.</summary>
	[RelayCommand(CanExecute = nameof(CanCreateNotifier))]
	private Task CreateNotifierAsync() => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client || SelectedProvider is not { } provider)
		{
			return;
		}

		var arguments = CreateArgumentFields.ToDictionary(f => f.Key, f => f.Value);
		var result = await client.Notifiers.CreateAsync(provider.Name, CreateDescription, arguments, ct)
			.ConfigureAwait(true);
		if (!result.Success)
		{
			_notifications.Warning("Shodan rejected the notifier arguments; double-check the required fields.");
			return;
		}

		CreateDescription = null;
		SelectedProvider = null;
		await ReloadNotifiersAsync(client, ct).ConfigureAwait(true);
		SelectedNotifier = Notifiers.FirstOrDefault(n => n.Id == result.Id);
		_notifications.Success($"Notifier created for provider \"{provider.Name}\".");
	});

	private bool HasSelectedNotifier() => SelectedNotifier is not null;

	/// <summary>Saves <see cref="EditArgumentFields" /> onto <see cref="SelectedNotifier" />.</summary>
	[RelayCommand(CanExecute = nameof(HasSelectedNotifier))]
	private Task UpdateNotifierAsync() => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client || SelectedNotifier is not { } notifier)
		{
			return;
		}

		var arguments = EditArgumentFields.ToDictionary(f => f.Key, f => f.Value);
		var updated = await client.Notifiers.UpdateAsync(notifier.Id, arguments, ct).ConfigureAwait(true);
		if (updated)
		{
			await ReplaceNotifierAsync(client, notifier.Id, ct).ConfigureAwait(true);
			_notifications.Success("Notifier updated.");
		}
		else
		{
			_notifications.Warning("Shodan did not update the notifier; double-check the required fields.");
		}
	});

	/// <summary>Deletes <see cref="SelectedNotifier" />.</summary>
	[RelayCommand(CanExecute = nameof(HasSelectedNotifier))]
	private Task DeleteNotifierAsync() => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client || SelectedNotifier is not { } notifier)
		{
			return;
		}

		var deleted = await client.Notifiers.DeleteAsync(notifier.Id, ct).ConfigureAwait(true);
		if (deleted)
		{
			Notifiers.Remove(notifier);
			SelectedNotifier = null;
			_notifications.Success("Notifier deleted.");
		}
		else
		{
			_notifications.Warning("Shodan did not delete the notifier.");
		}
	});

	private async Task ReloadNotifiersAsync(IShodanClient client, CancellationToken ct)
	{
		var notifiers = await client.Notifiers.ListAsync(ct).ConfigureAwait(true);
		Notifiers.Clear();
		foreach (var notifier in notifiers)
		{
			Notifiers.Add(notifier);
		}
	}

	private async Task ReplaceNotifierAsync(IShodanClient client, string id, CancellationToken ct)
	{
		var refreshed = await client.Notifiers.GetAsync(id, ct).ConfigureAwait(true);
		for (var i = 0; i < Notifiers.Count; i++)
		{
			if (Notifiers[i].Id != refreshed.Id)
			{
				continue;
			}

			Notifiers[i] = refreshed;
			break;
		}

		SelectedNotifier = refreshed;
	}
}
