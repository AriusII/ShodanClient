using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using ShodanClient.Application.Exceptions;

namespace ShodanClient.App.Services.Notifications;

/// <summary>
///     Default <see cref="INotificationService" />. Keeps an observable collection of toasts/banners
///     the shell renders, and translates every <see cref="ShodanException" /> subtype into the
///     appropriate visual treatment.
/// </summary>
public sealed class NotificationService : ObservableObject, INotificationService
{
	// Absolute safety net regardless of IsPersistent, so a pathological caller looping Report/Info
	// still can't grow this collection unbounded for the life of the app.
	private const int MaxItems = 50;

	// Toasts (non-persistent items) auto-expire after this long, mirroring a typical OS toast
	// lifetime, so a burst of background activity (e.g. periodic refreshes) doesn't leave stale
	// "success" text sitting in the status bar/toast host forever.
	private static readonly TimeSpan ToastLifetime = TimeSpan.FromSeconds(8);

	/// <inheritdoc />
	public ObservableCollection<NotificationItem> Items { get; } = [];

	/// <inheritdoc />
	public void Info(string message, string? title = null) =>
		Add(new NotificationItem(message, NotificationSeverity.Info, title));

	/// <inheritdoc />
	public void Success(string message, string? title = null) =>
		Add(new NotificationItem(message, NotificationSeverity.Success, title));

	/// <inheritdoc />
	public void Warning(string message, string? title = null, Action? action = null, string? actionLabel = null) =>
		Add(new NotificationItem(message, NotificationSeverity.Warning, title, action: action,
			actionLabel: actionLabel));

	/// <inheritdoc />
	public void Error(string message, string? title = null, Action? retry = null, TimeSpan? retryAfter = null)
	{
		var text = retryAfter is { } delay
			? string.Format(CultureInfo.InvariantCulture, "{0} (retry in {1:N0}s)", message, delay.TotalSeconds)
			: message;
		Add(new NotificationItem(text, NotificationSeverity.Error, title, action: retry,
			actionLabel: retry is null ? null : "Retry"));
	}

	/// <inheritdoc />
	public void Dismiss(NotificationItem item) => Items.Remove(item);

	/// <inheritdoc />
	public void Report(Exception exception)
	{
		switch (exception)
		{
			case ShodanAuthenticationException:
				Add(new NotificationItem(
					"Shodan rejected the API key. Open Settings to enter a valid key.",
					NotificationSeverity.Error,
					"API key rejected",
					true));
				break;

			case ShodanAccessDeniedException access:
				var surface = string.IsNullOrEmpty(access.Surface) ? "this feature" : access.Surface;
				Warning($"Your plan does not allow access to {surface}.", "Access denied");
				break;

			case ShodanNotFoundException:
				Info("Shodan has no data for this request.", "Not found");
				break;

			case ShodanRateLimitException rateLimit:
				Error("Shodan rate limit exceeded.", "Rate limited", retryAfter: rateLimit.RetryAfter);
				break;

			case ShodanServerException:
				Error("Shodan is currently having trouble processing this request.", "Server error");
				break;

			case ShodanSerializationException:
				Error("Received an unexpected response from Shodan.", "Unexpected response");
				break;

			case ShodanException other:
				Error(other.Message, "Request failed");
				break;

			default:
				Error(exception.Message, "Unexpected error");
				break;
		}
	}

	private void Add(NotificationItem item)
	{
		Items.Insert(0, item);

		while (Items.Count > MaxItems)
		{
			Items.RemoveAt(Items.Count - 1);
		}

		if (!item.IsPersistent)
		{
			_ = AutoDismissAsync(item);
		}
	}

	private async Task AutoDismissAsync(NotificationItem item)
	{
		await Task.Delay(ToastLifetime).ConfigureAwait(true);
		Dismiss(item);
	}
}
