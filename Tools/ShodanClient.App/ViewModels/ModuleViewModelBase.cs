using ShodanClient.App.Services.Notifications;
using ShodanClient.Application.Exceptions;

namespace ShodanClient.App.ViewModels;

/// <summary>
///     Base class for every module view model. Centralizes the busy flag, the module's display
///     title and the try/catch shape every network-backed operation should use.
/// </summary>
/// <remarks>Creates the module base with the shared notification service.</remarks>
public abstract class ModuleViewModelBase(INotificationService notifications) : ViewModelBase
{
	// A counter rather than a bare bool: a view model can have more than one [RelayCommand] funnel
	// through RunAsync concurrently (e.g. Dashboard's independent Refresh and Lookup actions). With a
	// bare bool, whichever call finished first would reset IsBusy=false while the other was still in
	// flight, prematurely re-enabling IsBusy-gated controls. With a counter, IsBusy only goes back to
	// false once every concurrent RunAsync call has completed.
	private int _busyCount;

	/// <summary>The module's display title, shown in the shell header.</summary>
	public string Title { get; protected set; } = string.Empty;

	/// <summary>Whether any <see cref="RunAsync" />-tracked operation on this view model is currently in flight.</summary>
	public bool IsBusy => _busyCount > 0;

	/// <summary>
	///     Runs <paramref name="operation" /> with a shared, reentrant-safe busy flag and centralized
	///     error handling: client-side validation failures are reported inline via
	///     <see cref="OnValidationError" />, every other <see cref="ShodanException" /> is routed to
	///     the global notification service.
	/// </summary>
	protected async Task RunAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
	{
		EnterBusy();
		try
		{
			await operation(ct).ConfigureAwait(true);
		}
		catch (ShodanRequestValidationException ex)
		{
			OnValidationError(ex);
		}
		catch (ShodanException ex)
		{
			notifications.Report(ex);
		}
		finally
		{
			ExitBusy();
		}
	}

	/// <summary>
	///     Called when <see cref="RunAsync" /> catches a client-side validation failure. The default
	///     implementation shows a warning toast; modules with an inline form error surface may override
	///     this to set a local property instead.
	/// </summary>
	protected virtual void OnValidationError(ShodanRequestValidationException ex) => notifications.Warning(ex.Message);

	private void EnterBusy()
	{
		if (Interlocked.Increment(ref _busyCount) == 1)
		{
			OnPropertyChanged(nameof(IsBusy));
		}
	}

	private void ExitBusy()
	{
		if (Interlocked.Decrement(ref _busyCount) == 0)
		{
			OnPropertyChanged(nameof(IsBusy));
		}
	}
}
