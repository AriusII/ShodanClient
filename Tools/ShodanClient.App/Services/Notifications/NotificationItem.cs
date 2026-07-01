namespace ShodanClient.App.Services.Notifications;

/// <summary>A single toast/banner notification shown by the shell.</summary>
/// <remarks>Creates a notification item.</remarks>
public sealed class NotificationItem(
	string message,
	NotificationSeverity severity,
	string? title = null,
	bool isPersistent = false,
	Action? action = null,
	string? actionLabel = null)
{
	/// <summary>The notification body text.</summary>
	public string Message { get; } = message;

	/// <summary>An optional short heading.</summary>
	public string? Title { get; } = title;

	/// <summary>The visual severity.</summary>
	public NotificationSeverity Severity { get; } = severity;

	/// <summary>
	///     Whether the notification should stay visible until dismissed (e.g. an authentication
	///     failure) rather than auto-expiring like a toast.
	/// </summary>
	public bool IsPersistent { get; } = isPersistent;

	/// <summary>An optional follow-up action (e.g. "Retry" or "Open Settings").</summary>
	public Action? Action { get; } = action;

	/// <summary>The label shown for <see cref="Action" />, if any.</summary>
	public string? ActionLabel { get; } = actionLabel;
}
