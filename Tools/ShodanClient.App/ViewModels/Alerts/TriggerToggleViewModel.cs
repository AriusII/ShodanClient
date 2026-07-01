using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ShodanClient.App.ViewModels.Alerts;

/// <summary>
///     A single row in the selected alert's trigger catalogue: whether the trigger is currently
///     enabled on that alert, plus an inline "ip:port" service-whitelist form. The actual network
///     calls live on <see cref="AlertsViewModel" /> (so they can share its <c>RunAsync</c> error
///     handling); this row only forwards user actions to it.
/// </summary>
/// <remarks>Creates a trigger row.</remarks>
public sealed partial class TriggerToggleViewModel(
	string name,
	string? description,
	bool isEnabled,
	Func<TriggerToggleViewModel, Task> onToggle,
	Func<TriggerToggleViewModel, Task> onIgnoreService,
	Func<TriggerToggleViewModel, Task> onUnignoreService) : ObservableObject
{
	/// <summary>Whether this trigger is currently enabled on the selected alert.</summary>
	[ObservableProperty]
	public partial bool IsEnabled { get; set; } = isEnabled;

	/// <summary>The <c>ip:port</c> service typed into the whitelist form for this trigger.</summary>
	[ObservableProperty]
	public partial string ServiceInput { get; set; } = string.Empty;

	/// <summary>The trigger's name, e.g. <c>malware</c>, <c>vulnerable</c>.</summary>
	public string Name { get; } = name;

	/// <summary>Human-readable description of the trigger, if any.</summary>
	public string? Description { get; } = description;

	/// <summary>Enables or disables this trigger on the selected alert.</summary>
	[RelayCommand]
	private Task ToggleAsync() => onToggle(this);

	private bool CanEditService() => !string.IsNullOrWhiteSpace(ServiceInput);

	/// <summary>Whitelists <see cref="ServiceInput" /> so it no longer fires this trigger.</summary>
	[RelayCommand(CanExecute = nameof(CanEditService))]
	private Task IgnoreServiceAsync() => onIgnoreService(this);

	/// <summary>Removes <see cref="ServiceInput" /> from this trigger's whitelist.</summary>
	[RelayCommand(CanExecute = nameof(CanEditService))]
	private Task UnignoreServiceAsync() => onUnignoreService(this);

	partial void OnServiceInputChanged(string value)
	{
		IgnoreServiceCommand.NotifyCanExecuteChanged();
		UnignoreServiceCommand.NotifyCanExecuteChanged();
	}
}
