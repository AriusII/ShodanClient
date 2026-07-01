using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using ShodanClient.Domain.Search;

namespace ShodanClient.App.Controls;

/// <summary>Reusable drill-down view for a single search result banner.</summary>
public partial class BannerDetailView : UserControl
{
	/// <summary>Defines the <see cref="OpenInExploitsCommand" /> property.</summary>
	public static readonly StyledProperty<ICommand?> OpenInExploitsCommandProperty =
		AvaloniaProperty.Register<BannerDetailView, ICommand?>(nameof(OpenInExploitsCommand));

	/// <summary>Creates the control.</summary>
	public BannerDetailView()
	{
		InitializeComponent();
		DataContextChanged += OnDataContextChanged;
	}

	/// <summary>
	///     Forwarded to every <see cref="VulnerabilityBadge" /> rendered for this banner's CVEs, so a
	///     hosting view (Streaming, Host Lookup) can wire a single "open in Exploits" command without
	///     this control needing its own reference to navigation.
	/// </summary>
	public ICommand? OpenInExploitsCommand
	{
		get => GetValue(OpenInExploitsCommandProperty);
		set => SetValue(OpenInExploitsCommandProperty, value);
	}

	/// <summary>
	///     Selects whichever of the Http/Ssl/Dns tabs actually has data for the bound <see cref="Banner" />
	///     (in that precedence order), instead of relying on <see cref="TabControl" />'s default
	///     <c>SelectedIndex="0"</c> — which would otherwise leave the detail pane looking blank for any
	///     non-HTTP banner (SSH/FTP/Telnet/etc.) where the first, invisible "Http" tab stays selected.
	/// </summary>
	private void OnDataContextChanged(object? sender, EventArgs e)
	{
		if (this.FindControl<TabControl>("ModuleTabs") is not { } tabs)
		{
			return;
		}

		tabs.SelectedIndex = DataContext switch
		{
			Banner { Http: not null } => 0,
			Banner { Ssl: not null } => 1,
			Banner { Dns: not null } => 2,
			_ => 0
		};
	}
}
