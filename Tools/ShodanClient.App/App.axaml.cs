using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ShodanClient.App.Composition;
using ShodanClient.App.ViewModels.Shell;
using ShodanClient.App.Views;

namespace ShodanClient.App;

// Fully qualified: this project's root namespace nests under "ShodanClient", which is also the
// SDK's root namespace and contains "ShodanClient.Application" — an unqualified "Application"
// would bind to that namespace instead of Avalonia's Application class.
public class App : Avalonia.Application
{
	public override void Initialize() => AvaloniaXamlLoader.Load(this);

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			var services = Program.AppHost.Services;

			DataTemplates.Add(new ViewLocator(ViewFactoryRegistry.Build()));

			var shellViewModel = services.GetRequiredService<ShellViewModel>();
			desktop.MainWindow = new MainWindow { DataContext = shellViewModel };
		}

		base.OnFrameworkInitializationCompleted();
	}
}
