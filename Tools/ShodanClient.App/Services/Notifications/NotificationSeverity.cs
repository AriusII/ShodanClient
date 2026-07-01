namespace ShodanClient.App.Services.Notifications;

/// <summary>Visual severity of a <see cref="NotificationItem" />.</summary>
public enum NotificationSeverity
{
	/// <summary>Neutral, informational message.</summary>
	Info,

	/// <summary>A successfully completed operation.</summary>
	Success,

	/// <summary>A recoverable problem the user should be aware of.</summary>
	Warning,

	/// <summary>A failure that prevented an operation from completing.</summary>
	Error
}
