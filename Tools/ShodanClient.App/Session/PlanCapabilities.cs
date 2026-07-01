namespace ShodanClient.App.Session;

/// <summary>
///     Tracks optimistic Enterprise-only feature gating, shared across the whole app as a singleton.
///     Both flags start <see langword="false" /> (nav items enabled) and flip to <see langword="true" />
///     the first time the corresponding surface responds with a 403 — <c>OrganizationViewModel</c> and
///     <c>BulkDataViewModel</c> set them right where they already catch their own
///     <c>ShodanAccessDeniedException</c> locally (the SDK's exception <c>Surface</c> is per-HTTP-channel,
///     e.g. <c>"Rest"</c>, not per-feature, so it can't be used to distinguish the two centrally) — while
///     <c>ShellViewModel</c> subscribes to <see cref="Changed" /> to disable/gray out the corresponding
///     nav item with an explanatory tooltip, everywhere in the app, immediately.
/// </summary>
public interface IPlanCapabilities
{
	/// <summary>Whether <c>Organization</c> returned an access-denied response.</summary>
	bool OrganizationAccessDenied { get; set; }

	/// <summary>Whether <c>BulkData</c> returned an access-denied response.</summary>
	bool BulkDataAccessDenied { get; set; }

	/// <summary>Raised whenever either flag changes value.</summary>
	event Action? Changed;
}

/// <inheritdoc cref="IPlanCapabilities" />
public sealed class PlanCapabilities : IPlanCapabilities
{
	/// <inheritdoc />
	public bool OrganizationAccessDenied
	{
		get;
		set => SetIfChanged(ref field, value);
	}

	/// <inheritdoc />
	public bool BulkDataAccessDenied
	{
		get;
		set => SetIfChanged(ref field, value);
	}

	/// <inheritdoc />
	public event Action? Changed;

	private void SetIfChanged(ref bool field, bool value)
	{
		if (field == value)
		{
			return;
		}

		field = value;
		Changed?.Invoke();
	}
}
