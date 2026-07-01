using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ShodanClient.App.Services.Notifications;

namespace ShodanClient.App.Converters;

/// <summary>Maps a <see cref="NotificationSeverity" /> to the accent stripe/icon color for a toast/banner.</summary>
public sealed class NotificationSeverityToBrushConverter : IValueConverter
{
	private static readonly IBrush Info = Brushes.SlateGray;
	private static readonly IBrush Success = new SolidColorBrush(Color.Parse("#2ECC71"));
	private static readonly IBrush Warning = Brushes.Goldenrod;
	private static readonly IBrush Error = new SolidColorBrush(Color.Parse("#FA3D3D"));

	/// <inheritdoc />
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value switch
		{
			NotificationSeverity.Success => Success,
			NotificationSeverity.Warning => Warning,
			NotificationSeverity.Error => Error,
			_ => Info
		};

	/// <inheritdoc />
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		AvaloniaProperty.UnsetValue;
}
