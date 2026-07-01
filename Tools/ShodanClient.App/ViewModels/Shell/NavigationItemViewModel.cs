using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using ShodanClient.App.Services.Navigation;

namespace ShodanClient.App.ViewModels.Shell;

/// <summary>A single entry in the shell's navigation sidebar.</summary>
public sealed partial class NavigationItemViewModel : ObservableObject
{
	/// <summary>Creates a navigation item.</summary>
	public NavigationItemViewModel(string label, FASymbol glyph, ModuleKey moduleKey)
	{
		Label = label;
		ModuleKey = moduleKey;
		IconSource = new FASymbolIconSource { Symbol = glyph };
	}

	[ObservableProperty] public partial string? DisabledReason { get; set; }

	[ObservableProperty] public partial bool IsEnabled { get; set; } = true;

	/// <summary>The display label shown next to the icon.</summary>
	public string Label { get; }

	/// <summary>The module this item navigates to.</summary>
	public ModuleKey ModuleKey { get; }

	/// <summary>The icon shown for this item.</summary>
	public FAIconSource IconSource { get; }
}
