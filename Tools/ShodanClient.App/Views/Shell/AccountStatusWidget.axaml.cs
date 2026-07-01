using Avalonia.Controls;

namespace ShodanClient.App.Views.Shell;

/// <summary>
///     Always-visible sidebar widget showing the linked account's plan/credits, a manual refresh
///     action and the "Switch account" profile-management flyout. Bound to
///     <see cref="ShodanClient.App.ViewModels.Shell.AccountStatusWidgetViewModel" />.
/// </summary>
public partial class AccountStatusWidget : UserControl
{
	/// <summary>Creates the widget.</summary>
	public AccountStatusWidget()
	{
		InitializeComponent();
	}
}
