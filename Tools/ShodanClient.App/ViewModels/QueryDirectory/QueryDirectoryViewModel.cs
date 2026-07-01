using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.Navigation;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.Domain.Directory;

namespace ShodanClient.App.ViewModels.QueryDirectory;

/// <summary>
///     Directory of saved search queries, with a "use this query" hand-off to Search. Owned by a
///     later wave of agents; this is the registered, navigable skeleton.
/// </summary>
/// <remarks>
///     Named <c>QueryDirectory</c> rather than <c>Directory</c> (the
///     <see cref="ShodanClient.App.Services.Navigation.ModuleKey" />
///     value) to avoid shadowing <see cref="System.IO.Directory" />, which is in scope everywhere via
///     the project's implicit usings.
/// </remarks>
public sealed partial class QueryDirectoryViewModel : ModuleViewModelBase
{
	private readonly INotificationService _notifications;

	private QueryMode _lastMode = QueryMode.Browse;

	/// <summary>
	///     The largest match count observed on a single page so far, used as a stand-in for the
	///     server's (undocumented) page size when deciding whether a further page exists.
	/// </summary>
	private int _observedPageSize = 1;

	/// <summary>Creates the Directory module view model.</summary>
	public QueryDirectoryViewModel(
		INotificationService notifications,
		IShodanClientAccessor accessor,
		INavigationService navigation)
		: base(notifications)
	{
		_notifications = notifications;
		Accessor = accessor;
		Navigation = navigation;
		Title = "Directory";

		_ = RunAsync(async ct =>
		{
			await LoadPopularTagsCoreAsync(ct).ConfigureAwait(true);
			await LoadBrowseAsync(ct).ConfigureAwait(true);
		});
	}

	[ObservableProperty] public partial bool HasMoreResults { get; set; }

	[ObservableProperty] public partial int Page { get; set; } = 1;

	[ObservableProperty] public partial string SearchTerm { get; set; } = string.Empty;

	[ObservableProperty] public partial int SelectedTabIndex { get; set; }

	[ObservableProperty] public partial string? SortField { get; set; }

	[ObservableProperty] public partial string? SortOrder { get; set; }

	[ObservableProperty] public partial long TotalResults { get; set; }

	/// <summary>The active Shodan client accessor.</summary>
	public IShodanClientAccessor Accessor { get; }

	/// <summary>The shell's navigation service.</summary>
	public INavigationService Navigation { get; }

	/// <summary>Fields the Browse tab can sort on.</summary>
	public IReadOnlyList<string> SortFields { get; } = ["votes", "timestamp"];

	/// <summary>Sort directions the Browse tab supports.</summary>
	public IReadOnlyList<string> SortOrders { get; } = ["asc", "desc"];

	/// <summary>The saved queries currently displayed, populated by whichever tab last ran.</summary>
	public ObservableCollection<SavedQuery> Results { get; } = [];

	/// <summary>The most popular tags across saved queries, seeded once and clickable to search.</summary>
	public ObservableCollection<QueryTag> PopularTags { get; } = [];

	/// <summary>Whether <see cref="PreviousPageCommand" /> would do anything (there is a previous page).</summary>
	public bool CanGoToPreviousPage => Page > 1;

	partial void OnPageChanged(int value) => OnPropertyChanged(nameof(CanGoToPreviousPage));

	/// <summary>Browses the directory (<c>ListQueriesAsync</c>) using <see cref="SortField" />/<see cref="SortOrder" />.</summary>
	[RelayCommand]
	private Task BrowseAsync(CancellationToken cancellationToken)
	{
		Page = 1;
		_observedPageSize = 1;
		return RunAsync(LoadBrowseAsync, cancellationToken);
	}

	/// <summary>Searches the directory (<c>SearchQueriesAsync</c>) for <see cref="SearchTerm" />.</summary>
	[RelayCommand]
	private Task SearchAsync(CancellationToken cancellationToken)
	{
		var term = SearchTerm.Trim();
		if (term.Length == 0)
		{
			_notifications.Warning("Enter a search term.");
			return Task.CompletedTask;
		}

		SearchTerm = term;
		Page = 1;
		_observedPageSize = 1;
		return RunAsync(LoadSearchAsync, cancellationToken);
	}

	/// <summary>
	///     Seeds <see cref="SearchTerm" /> from a popular tag, switches to the Search tab so the change
	///     is visible, and immediately searches for it.
	/// </summary>
	[RelayCommand]
	private Task SelectTagAsync(QueryTag tag, CancellationToken cancellationToken)
	{
		SearchTerm = tag.Value;
		Page = 1;
		_observedPageSize = 1;
		SelectedTabIndex = 1;
		return RunAsync(LoadSearchAsync, cancellationToken);
	}

	/// <summary>Reloads the previous page, if any.</summary>
	[RelayCommand]
	private Task PreviousPageAsync(CancellationToken cancellationToken)
	{
		if (Page <= 1)
		{
			return Task.CompletedTask;
		}

		Page--;
		return RunAsync(ReloadCurrentPageAsync, cancellationToken);
	}

	/// <summary>Loads the next page, if <see cref="HasMoreResults" />.</summary>
	[RelayCommand]
	private Task NextPageAsync(CancellationToken cancellationToken)
	{
		if (!HasMoreResults)
		{
			return Task.CompletedTask;
		}

		Page++;
		return RunAsync(ReloadCurrentPageAsync, cancellationToken);
	}

	/// <summary>Deep-links into Search, raw-query mode, with <paramref name="query" />'s query string.</summary>
	[RelayCommand]
	private void UseQuery(SavedQuery query) => Navigation.NavigateTo(ModuleKey.Search, query.Query);

	private Task ReloadCurrentPageAsync(CancellationToken cancellationToken) =>
		_lastMode == QueryMode.Search ? LoadSearchAsync(cancellationToken) : LoadBrowseAsync(cancellationToken);

	private async Task LoadBrowseAsync(CancellationToken cancellationToken)
	{
		if (Accessor.Client is not { } client)
		{
			_notifications.Warning("Connect a Shodan API key first.");
			return;
		}

		_lastMode = QueryMode.Browse;
		var result = await client.Directory.ListQueriesAsync(Page, SortField, SortOrder, cancellationToken)
			.ConfigureAwait(true);
		ApplyResult(result);
	}

	private async Task LoadSearchAsync(CancellationToken cancellationToken)
	{
		if (Accessor.Client is not { } client)
		{
			_notifications.Warning("Connect a Shodan API key first.");
			return;
		}

		var term = SearchTerm.Trim();
		if (term.Length == 0)
		{
			return;
		}

		_lastMode = QueryMode.Search;
		var result = await client.Directory.SearchQueriesAsync(term, Page, cancellationToken).ConfigureAwait(true);
		ApplyResult(result);
	}

	private async Task LoadPopularTagsCoreAsync(CancellationToken cancellationToken)
	{
		if (Accessor.Client is not { } client)
		{
			return;
		}

		var tags = await client.Directory.ListTagsAsync(20, cancellationToken).ConfigureAwait(true);
		PopularTags.Clear();
		foreach (var tag in tags)
		{
			PopularTags.Add(tag);
		}
	}

	private void ApplyResult(SavedQueryResult result)
	{
		Results.Clear();
		foreach (var match in result.Matches)
		{
			Results.Add(match);
		}

		TotalResults = result.Total;

		// The directory endpoints don't report a page size, so derive one from the largest page seen
		// so far and use it to estimate whether another page exists — this also naturally disables
		// "Next" once a page comes back empty.
		var matchCount = result.Matches.Count;
		if (matchCount > _observedPageSize)
		{
			_observedPageSize = matchCount;
		}

		var shownSoFar = (long)(Page - 1) * _observedPageSize + matchCount;
		HasMoreResults = matchCount > 0 && shownSoFar < TotalResults;
	}

	private enum QueryMode
	{
		Browse,
		Search
	}
}
