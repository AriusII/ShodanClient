using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ShodanClient.App.ViewModels;

namespace ShodanClient.App;

/// <summary>
///     AOT-safe replacement for the scaffold's reflection-based view locator. Resolves a view model
///     to its view via a closed dictionary built once by <see cref="Composition.ViewFactoryRegistry" />
///     — no <c>Type.GetType</c>, no <c>Activator.CreateInstance</c>.
/// </summary>
public sealed class ViewLocator : IDataTemplate
{
	private readonly IReadOnlyDictionary<Type, Func<Control>> _factories;

	/// <summary>Creates the locator with the closed view-model-to-view factory map.</summary>
	public ViewLocator(IReadOnlyDictionary<Type, Func<Control>> factories)
	{
		_factories = factories;
	}

	/// <inheritdoc />
	public Control? Build(object? param)
	{
		if (param is null)
		{
			return null;
		}

		return _factories.TryGetValue(param.GetType(), out var factory)
			? factory()
			: new TextBlock { Text = $"No view registered for {param.GetType().FullName}." };
	}

	/// <inheritdoc />
	public bool Match(object? data) => data is ViewModelBase;
}
