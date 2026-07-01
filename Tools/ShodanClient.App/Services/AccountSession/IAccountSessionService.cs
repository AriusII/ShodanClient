using ShodanClient.App.Session;

namespace ShodanClient.App.Services.AccountSession;

/// <summary>
///     Tracks the linked account's plan and credit status, merging <c>Account.GetProfileAsync()</c>
///     and <c>ApiInfo.GetAsync()</c> into a single <see cref="AccountSnapshot" /> that
///     <c>AccountStatusWidget</c> binds to directly.
/// </summary>
public interface IAccountSessionService
{
	/// <summary>
	///     The most recently fetched snapshot, or <see langword="null" /> before the first refresh
	///     (or once the client is detached).
	/// </summary>
	AccountSnapshot? Current { get; }

	/// <summary>Whether a refresh is currently in flight.</summary>
	bool IsRefreshing { get; }

	/// <summary>When <see cref="Current" /> was last successfully refreshed.</summary>
	DateTimeOffset? LastRefreshedAt { get; }

	/// <summary>
	///     Refreshes <see cref="Current" />. Concurrent callers share a single in-flight request; unless
	///     <paramref name="force" /> is <see langword="true" />, calls within 10 seconds of the last
	///     successful refresh are coalesced into a no-op.
	/// </summary>
	Task RefreshAsync(bool force = false, CancellationToken cancellationToken = default);
}
