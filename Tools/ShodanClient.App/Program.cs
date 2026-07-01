using Avalonia;
using Microsoft.Extensions.Hosting;
using ShodanClient.App.Composition;

namespace ShodanClient.App;

internal sealed class Program
{
	/// <summary>
	///     The composition root's host, built and started before the Avalonia lifetime and stopped
	///     after it. Exposed so <see cref="App" /> can resolve services once the framework has
	///     initialized.
	/// </summary>
	internal static IHost AppHost { get; private set; } = null!;

	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args)
	{
		AppHost = CompositionRoot.Build();
		AppHost.Start();
		try
		{
			BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
		}
		finally
		{
			AppHost.StopAsync().GetAwaiter().GetResult();
			AppHost.Dispose();
		}
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp() =>
		AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();
}
