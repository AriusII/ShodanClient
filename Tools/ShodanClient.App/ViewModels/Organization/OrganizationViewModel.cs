using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.App.Session;
using ShodanClient.Application.Exceptions;
// NOTE: this file's enclosing namespace is `ShodanClient.App.ViewModels.Organization`, whose direct
// ancestor `ShodanClient.App.ViewModels` also has a nested namespace literally called
// `Organization` (this very file's namespace) — so a bare, unqualified `Organization` identifier
// would bind to that sibling namespace instead of the SDK's `Organization` domain type and fail
// with CS0118 (the exact same pitfall documented on `IOrganizationService`). Alias the domain type
// under a non-colliding name instead of importing it unqualified.
using OrganizationProfile = ShodanClient.Domain.Organization.Organization;

namespace ShodanClient.App.ViewModels.Organization;

/// <summary>
///     Enterprise organization profile and member management (<c>GET/PUT/DELETE /org*</c>).
/// </summary>
public sealed partial class OrganizationViewModel : ModuleViewModelBase
{
	private readonly IPlanCapabilities _planCapabilities;

	/// <summary>Creates the Organization module view model and kicks off the initial load.</summary>
	/// <param name="planCapabilities">
	///     The shared, cross-module Enterprise-capability cache (also consumed by <c>ShellViewModel</c>
	///     to disable/gray out this module's nav item once <see cref="IPlanCapabilities.OrganizationAccessDenied" />
	///     flips).
	/// </param>
	public OrganizationViewModel(
		INotificationService notifications,
		IShodanClientAccessor accessor,
		IPlanCapabilities planCapabilities)
		: base(notifications)
	{
		Accessor = accessor;
		_planCapabilities = planCapabilities;
		Title = "Organization";

		// The shared, cross-module flag may already be known from an earlier 403 this session (e.g.
		// a previous visit to this module, or another Enterprise-gated surface reporting one first);
		// reflect that immediately instead of waiting for a fresh, doomed request to confirm it again.
		RequiresEnterprise = _planCapabilities.OrganizationAccessDenied;

		LoadCommand.Execute(null);
	}

	/// <summary>The username or email typed into the "add member" form.</summary>
	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(AddMemberCommand))]
	public partial string NewMemberUser { get; set; } = string.Empty;

	/// <summary>Whether to notify the new member by email when adding them.</summary>
	[ObservableProperty]
	public partial bool NotifyNewMember { get; set; }

	/// <summary>The organization's profile, or <see langword="null" /> if not loaded/available.</summary>
	[ObservableProperty]
	public partial OrganizationProfile? Org { get; set; }

	/// <summary>
	///     Set when <c>GET /org</c> responded with a 403: the calling account isn't on an Enterprise
	///     plan. This is an expected, common outcome for non-Enterprise users, not a bug, so it is
	///     surfaced as a local empty-state rather than a generic error toast.
	/// </summary>
	[ObservableProperty]
	public partial bool RequiresEnterprise { get; set; }

	/// <summary>The active Shodan client accessor.</summary>
	public IShodanClientAccessor Accessor { get; }

	/// <summary>Members and admins, merged into a single display list (see <see cref="OrgMemberRow" />).</summary>
	public ObservableCollection<OrgMemberRow> MemberRows { get; } = [];

	/// <summary>Reloads the organization's profile and member list.</summary>
	[RelayCommand]
	private Task LoadAsync(CancellationToken cancellationToken) => RunAsync(LoadCoreAsync, cancellationToken);

	private bool CanAddMember() => !string.IsNullOrWhiteSpace(NewMemberUser);

	/// <summary>Adds (or upgrades) a member, then reloads the member list.</summary>
	[RelayCommand(CanExecute = nameof(CanAddMember))]
	private Task AddMemberAsync(CancellationToken cancellationToken) => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client || string.IsNullOrWhiteSpace(NewMemberUser))
		{
			return;
		}

		await client.Organization.AddMemberAsync(NewMemberUser, NotifyNewMember, ct).ConfigureAwait(true);
		NewMemberUser = string.Empty;
		NotifyNewMember = false;
		await LoadCoreAsync(ct).ConfigureAwait(true);
	}, cancellationToken);

	/// <summary>Removes a member, then reloads the member list.</summary>
	[RelayCommand]
	private Task RemoveMemberAsync(OrgMemberRow? row, CancellationToken cancellationToken) => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client || row is null)
		{
			return;
		}

		await client.Organization.RemoveMemberAsync(row.Username, ct).ConfigureAwait(true);
		await LoadCoreAsync(ct).ConfigureAwait(true);
	}, cancellationToken);

	private async Task LoadCoreAsync(CancellationToken ct)
	{
		if (Accessor.Client is not { } client)
		{
			return;
		}

		if (_planCapabilities.OrganizationAccessDenied)
		{
			// Already confirmed denied earlier this session; short-circuit straight to the empty
			// state instead of re-issuing a request that's near-certain to 403 again.
			Org = null;
			RequiresEnterprise = true;
			RebuildMemberRows();
			return;
		}

		try
		{
			Org = await client.Organization.GetAsync(ct).ConfigureAwait(true);
			RequiresEnterprise = false;
			_planCapabilities.OrganizationAccessDenied = false;
		}
		catch (ShodanAccessDeniedException)
		{
			Org = null;
			RequiresEnterprise = true;
			_planCapabilities.OrganizationAccessDenied = true;
		}

		RebuildMemberRows();
	}

	private void RebuildMemberRows()
	{
		MemberRows.Clear();

		if (Org is null)
		{
			return;
		}

		var adminUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var admin in Org.Admins)
		{
			adminUsernames.Add(admin.Username);
		}

		var seenUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var member in Org.Members)
		{
			seenUsernames.Add(member.Username);
			MemberRows.Add(new OrgMemberRow(member.Username, member.Email, adminUsernames.Contains(member.Username)));
		}

		// Some accounts may list an admin that isn't otherwise present in the member list.
		foreach (var admin in Org.Admins)
		{
			if (seenUsernames.Add(admin.Username))
			{
				MemberRows.Add(new OrgMemberRow(admin.Username, admin.Email, true));
			}
		}
	}
}

/// <summary>A single row of <see cref="OrganizationViewModel.MemberRows" />: a member merged with its admin status.</summary>
/// <param name="Username">The member's Shodan username.</param>
/// <param name="Email">The member's email address, if known.</param>
/// <param name="IsAdmin">Whether the member is one of the organization's administrators.</param>
public sealed record OrgMemberRow(string Username, string? Email, bool IsAdmin);
