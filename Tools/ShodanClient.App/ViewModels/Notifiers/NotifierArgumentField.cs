using CommunityToolkit.Mvvm.ComponentModel;

namespace ShodanClient.App.ViewModels.Notifiers;

/// <summary>
///     A single editable provider argument, rendered dynamically from a
///     <see cref="ShodanClient.Domain.Notifiers.NotifierProvider.Required" /> list (or from an
///     existing notifier's <see cref="ShodanClient.Domain.Notifiers.Notifier.Args" /> keys when
///     editing).
/// </summary>
/// <remarks>Creates an argument field.</remarks>
public sealed partial class NotifierArgumentField(string key, string value = "") : ObservableObject
{
	/// <summary>The value the user typed for this argument.</summary>
	[ObservableProperty]
	public partial string Value { get; set; } = value;

	/// <summary>The argument's name, e.g. <c>to</c> for the <c>email</c> provider.</summary>
	public string Key { get; } = key;
}
