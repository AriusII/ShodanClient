using Avalonia;
using Avalonia.Controls;

namespace ShodanClient.App.Controls;

/// <summary>A small reusable chip displaying a single tag string.</summary>
public partial class TagPill : UserControl
{
	/// <summary>Defines the <see cref="Text" /> property.</summary>
	public static readonly StyledProperty<string?> TextProperty =
		AvaloniaProperty.Register<TagPill, string?>(nameof(Text));

	/// <summary>Creates the control.</summary>
	public TagPill()
	{
		InitializeComponent();
	}

	/// <summary>The tag text shown in the pill.</summary>
	public string? Text
	{
		get => GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}
}
