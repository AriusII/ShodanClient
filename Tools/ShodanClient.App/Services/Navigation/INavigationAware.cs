namespace ShodanClient.App.Services.Navigation;

/// <summary>
///     Implemented by module view models that want to react to the parameter passed to
///     <see cref="INavigationService.NavigateTo" /> when they become the current module (for example
///     Directory handing a saved query string to Search).
/// </summary>
public interface INavigationAware<in TParam> : INavigationAwareDispatch
{
	void INavigationAwareDispatch.OnNavigatedTo(object? parameter) => OnNavigatedTo((TParam)parameter!);

	/// <summary>Called after the view model is resolved and becomes the current module.</summary>
	void OnNavigatedTo(TParam parameter);
}

/// <summary>
///     Non-generic dispatch surface bridging <see cref="INavigationAware{TParam}" /> so
///     <see cref="NavigationService" /> can invoke it without reflection (the closed generic
///     instantiation is resolved at compile time inside each implementing view model).
/// </summary>
public interface INavigationAwareDispatch
{
	/// <summary>Invokes the typed <c>OnNavigatedTo</c> overload via the default interface implementation.</summary>
	void OnNavigatedTo(object? parameter);
}
