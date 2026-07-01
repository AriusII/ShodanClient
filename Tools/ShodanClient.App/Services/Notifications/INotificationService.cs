using System.Collections.ObjectModel;
using ShodanClient.Application.Exceptions;

namespace ShodanClient.App.Services.Notifications;

/// <summary>
///     Central sink for user-facing notifications: toasts for transient status and persistent
///     banners for things that need action (e.g. an invalid API key). Also the single place that
///     translates a caught <see cref="ShodanException" /> into user-facing text.
/// </summary>
public interface INotificationService
{
	/// <summary>The currently visible notifications, most recent first.</summary>
	ObservableCollection<NotificationItem> Items { get; }

	/// <summary>Shows a neutral informational toast.</summary>
	void Info(string message, string? title = null);

	/// <summary>Shows a success toast.</summary>
	void Success(string message, string? title = null);

	/// <summary>Shows a warning toast, optionally with a follow-up action.</summary>
	void Warning(string message, string? title = null, Action? action = null, string? actionLabel = null);

	/// <summary>Shows an error toast, optionally with a retry action and a suggested retry delay.</summary>
	void Error(string message, string? title = null, Action? retry = null, TimeSpan? retryAfter = null);

	/// <summary>Removes a previously shown notification.</summary>
	void Dismiss(NotificationItem item);

	/// <summary>
	///     Translates a caught <see cref="ShodanException" /> into an appropriate notification (a
	///     persistent banner for authentication failures, a toast with countdown for rate limits, …)
	///     and shows it.
	/// </summary>
	void Report(Exception exception);
}
