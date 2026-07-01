using System.Globalization;
using Avalonia.Controls;
using Avalonia.Threading;

namespace ShodanClient.App.Views.Shell;

/// <summary>Thin bottom bar: app name, latest notification summary, and a live clock.</summary>
public partial class StatusBarView : UserControl
{
	private readonly DispatcherTimer _clockTimer;

	/// <summary>Creates the view.</summary>
	public StatusBarView()
	{
		InitializeComponent();

		_clockTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(1) };
		_clockTimer.Tick += (_, _) => UpdateClock();
		_clockTimer.Start();
		UpdateClock();
	}

	private void UpdateClock()
	{
		var clock = this.FindControl<TextBlock>("ClockText");
		if (clock is null)
		{
			return;
		}

		clock.Text = DateTimeOffset.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
	}
}
