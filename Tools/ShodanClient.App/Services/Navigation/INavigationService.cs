using System.ComponentModel;
using ShodanClient.App.ViewModels;

namespace ShodanClient.App.Services.Navigation;

/// <summary>
///     Resolves and caches module view models by <see cref="ModuleKey" /> and exposes the one
///     currently displayed by the shell. Implements <see cref="INotifyPropertyChanged" /> so
///     <c>ShellViewModel</c> can forward <see cref="CurrentViewModel" /> changes.
/// </summary>
public interface INavigationService : INotifyPropertyChanged
{
	/// <summary>The view model currently hosted by the shell's content area, if any.</summary>
	ViewModelBase? CurrentViewModel { get; }

	/// <summary>
	///     Navigates to the module identified by <paramref name="key" />, resolving (and caching) its
	///     view model on first use. If the resolved view model implements
	///     <see cref="INavigationAware{TParam}" />, <c>OnNavigatedTo</c> is invoked with
	///     <paramref name="parameter" />.
	/// </summary>
	void NavigateTo(ModuleKey key, object? parameter = null);
}
