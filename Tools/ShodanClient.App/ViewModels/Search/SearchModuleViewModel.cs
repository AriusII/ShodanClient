using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.AccountSession;
using ShodanClient.App.Services.Navigation;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.Application.Exceptions;
using ShodanClient.Domain.Search;

namespace ShodanClient.App.ViewModels.Search;

/// <summary>
///     Shodan search builder, results grid and facets — the flagship module. Composes a query from
///     either a chip editor (free text + <c>name:value</c> filter chips) or a raw query escape hatch,
///     validates it live against <c>ParseQueryAsync</c>, and lets the user drill from a facet bucket
///     or a result row straight back into a refined search or a banner's full detail.
/// </summary>
public sealed partial class SearchModuleViewModel : ModuleViewModelBase, INavigationAware<string?>
{
	private static readonly TimeSpan ParseDebounceDelay = TimeSpan.FromMilliseconds(400);

	private readonly IAccountSessionService _accountSession;
	private readonly INotificationService _notifications;

	private CancellationTokenSource? _parseDebounceCts;

	private bool _suggestionsLoaded;

	/// <summary>Creates the Search module view model.</summary>
	public SearchModuleViewModel(
		INotificationService notifications,
		IShodanClientAccessor accessor,
		INavigationService navigation,
		IAccountSessionService accountSession)
		: base(notifications)
	{
		_notifications = notifications;
		_accountSession = accountSession;
		Accessor = accessor;
		Navigation = navigation;
		Title = "Search";

		AppliedFilters.CollectionChanged += (_, _) => OnEffectiveQueryInputsChanged();
	}

	/// <summary>The 1-based page of <see cref="CurrentResult" /> currently displayed.</summary>
	[ObservableProperty]
	public partial int CurrentPage { get; set; } = 1;

	/// <summary>The most recent full search result page.</summary>
	[ObservableProperty]
	public partial SearchResult? CurrentResult { get; set; }

	/// <summary>The free-text portion of the composed query (chip mode only).</summary>
	[ObservableProperty]
	public partial string? FreeTextTerm { get; set; }

	/// <summary>Whether the last query parse reported any errors.</summary>
	[ObservableProperty]
	public partial bool HasQueryErrors { get; set; }

	/// <summary>Whether the user is editing a raw query string instead of the chip builder.</summary>
	[ObservableProperty]
	public partial bool IsRawQueryMode { get; set; }

	/// <summary>The most recent result of <c>ParseQueryAsync</c> for the effective query, if any.</summary>
	[ObservableProperty]
	public partial QueryTokenParse? LastParse { get; set; }

	/// <summary>Pending facet name for the "request a facet" row.</summary>
	[ObservableProperty]
	public partial string? NewFacetName { get; set; }

	/// <summary>Pending filter name for the chip editor's "add filter" row.</summary>
	[ObservableProperty]
	public partial string? NewFilterName { get; set; }

	/// <summary>Pending filter value for the chip editor's "add filter" row.</summary>
	[ObservableProperty]
	public partial string? NewFilterValue { get; set; }

	/// <summary>The free, credit-less total from the last <c>CountAsync</c> preview.</summary>
	[ObservableProperty]
	public partial long? PreviewTotal { get; set; }

	/// <summary>A local, non-toast validation message (e.g. an empty query) shown next to the Search button.</summary>
	[ObservableProperty]
	public partial string? QueryValidationError { get; set; }

	/// <summary>The raw query string, used when <see cref="IsRawQueryMode" /> is <see langword="true" />.</summary>
	[ObservableProperty]
	public partial string? RawQueryOverride { get; set; }

	/// <summary>The banner shown in the drill-down detail panel, or <see langword="null" /> if none is selected.</summary>
	[ObservableProperty]
	public partial Banner? SelectedBannerForDetail { get; set; }

	/// <summary>The active Shodan client accessor.</summary>
	public IShodanClientAccessor Accessor { get; }

	/// <summary>The shell's navigation service.</summary>
	public INavigationService Navigation { get; }

	/// <summary>The filter chips currently applied to the query (chip-mode only).</summary>
	public ObservableCollection<FilterChip> AppliedFilters { get; } = [];

	/// <summary>Filter names known to Shodan, loaded once per activation for autocomplete.</summary>
	public ObservableCollection<string> FilterNameSuggestions { get; } = [];

	/// <summary>Facet names known to Shodan, loaded once per activation for autocomplete.</summary>
	public ObservableCollection<string> FacetNameSuggestions { get; } = [];

	/// <summary>Facet names requested on the next search/preview, summarized alongside the results.</summary>
	public ObservableCollection<string> RequestedFacets { get; } = [];

	/// <summary>The current page's banners.</summary>
	public ObservableCollection<Banner> Matches { get; } = [];

	/// <summary>Per-facet breakdowns of <see cref="CurrentResult" />, ready for the facets side panel.</summary>
	public ObservableCollection<FacetGroupDisplay> FacetGroups { get; } = [];

	/// <summary>The query that will actually be sent to Shodan: the raw override, or the composed chips.</summary>
	public string EffectiveQuery => IsRawQueryMode ? RawQueryOverride ?? string.Empty : ComposeChipQuery();

	/// <summary>A human-readable summary of how Shodan currently interprets <see cref="EffectiveQuery" />.</summary>
	public string InterpretedSummary
	{
		get
		{
			if (LastParse is not { } parse)
			{
				return "Type a query to see how Shodan interprets it.";
			}

			var parts = new List<string>();
			if (!string.IsNullOrWhiteSpace(parse.SearchTerm))
			{
				parts.Add($"free text \"{parse.SearchTerm}\"");
			}

			if (parse.Filters.Count > 0)
			{
				parts.Add($"filters: {string.Join(", ", parse.Filters)}");
			}

			return parts.Count > 0
				? $"Interpreted as {string.Join(" + ", parts)}."
				: "Interpreted as an empty query.";
		}
	}

	/// <summary>The parse errors for the last effective query, joined for display.</summary>
	public string QueryErrorsSummary =>
		LastParse is { Errors.Count: > 0 } parse ? string.Join(" · ", parse.Errors) : string.Empty;

	/// <summary>
	///     Whether <see cref="EffectiveQuery" /> has been parsed at least once. Used to distinguish the
	///     initial empty state (neutral banner) from a query that actually parsed cleanly (success banner).
	/// </summary>
	public bool HasParsedQuery => LastParse is not null;

	/// <summary>Whether the green "parsed cleanly" banner should show, as opposed to the neutral initial state.</summary>
	public bool ShowSuccessBanner => LastParse is not null && !HasQueryErrors;

	/// <summary>
	///     A curated description/example for <see cref="NewFilterName" />, if it matches one of
	///     <see cref="KnownFilters" />, shown as a hint below the "add filter" row.
	/// </summary>
	public string? NewFilterHint =>
		!string.IsNullOrWhiteSpace(NewFilterName) && KnownFilters.TryGet(NewFilterName.Trim()) is { } info
			? $"{info.Description} (e.g. {info.Example})"
			: null;

	/// <inheritdoc />
	public void OnNavigatedTo(string? parameter)
	{
		if (!string.IsNullOrWhiteSpace(parameter))
		{
			IsRawQueryMode = true;
			RawQueryOverride = parameter;
		}

		if (_suggestionsLoaded)
		{
			return;
		}

		_suggestionsLoaded = true;
		_ = LoadSuggestionsAsync();
	}

	partial void OnFreeTextTermChanged(string? value) => OnEffectiveQueryInputsChanged();

	partial void OnNewFilterNameChanged(string? value) => OnPropertyChanged(nameof(NewFilterHint));

	partial void OnIsRawQueryModeChanged(bool value)
	{
		// Switching into raw mode composes the current chip editor's contents into the raw text box,
		// so toggling doesn't lose the query the user already built (whether toggled via the view's
		// ToggleSwitch or, e.g., a future direct assignment of this property).
		if (value)
		{
			RawQueryOverride = ComposeChipQuery();
		}

		OnEffectiveQueryInputsChanged();
	}

	partial void OnRawQueryOverrideChanged(string? value) => OnEffectiveQueryInputsChanged();

	partial void OnLastParseChanged(QueryTokenParse? value)
	{
		OnPropertyChanged(nameof(InterpretedSummary));
		OnPropertyChanged(nameof(QueryErrorsSummary));
		OnPropertyChanged(nameof(HasParsedQuery));
		OnPropertyChanged(nameof(ShowSuccessBanner));
	}

	partial void OnHasQueryErrorsChanged(bool value) => OnPropertyChanged(nameof(ShowSuccessBanner));

	/// <summary>Runs the search for <see cref="EffectiveQuery" />, resetting to the first page.</summary>
	[RelayCommand]
	private async Task SearchAsync()
	{
		await RunAsync(async ct =>
		{
			var client = Accessor.Client;
			if (client is null)
			{
				return;
			}

			var query = EffectiveQuery;
			if (string.IsNullOrWhiteSpace(query))
			{
				throw new ShodanRequestValidationException(
					"Enter a search term or at least one filter before searching.");
			}

			QueryValidationError = null;
			var result = await client.Search.SearchAsync(query, BuildFacetsParameter(), 1, ct).ConfigureAwait(true);
			CurrentPage = 1;
			SelectedBannerForDetail = null;
			ApplyResult(result);

			// A search that includes a filter (or any page beyond the first) always spends a query
			// credit, so force the credit display to refresh now rather than silently coalescing
			// within AccountSessionService's 10-second window.
			await _accountSession.RefreshAsync(true, ct).ConfigureAwait(true);
		}).ConfigureAwait(true);
	}

	/// <summary>Previews the total match count for <see cref="EffectiveQuery" /> without consuming credits.</summary>
	[RelayCommand]
	private async Task PreviewCountAsync()
	{
		await RunAsync(async ct =>
		{
			var client = Accessor.Client;
			if (client is null)
			{
				PreviewTotal = null;
				return;
			}

			var query = EffectiveQuery;
			if (string.IsNullOrWhiteSpace(query))
			{
				PreviewTotal = null;
				throw new ShodanRequestValidationException(
					"Enter a search term or at least one filter before previewing a count.");
			}

			QueryValidationError = null;
			var count = await client.Search.CountAsync(query, BuildFacetsParameter(), ct).ConfigureAwait(true);
			PreviewTotal = count.Total;
		}).ConfigureAwait(true);
	}

	/// <summary>Warns about the credit cost of paging further, then loads the next page on confirmation.</summary>
	[RelayCommand]
	private void LoadNextPage()
	{
		if (CurrentResult is null)
		{
			return;
		}

		var nextPage = CurrentPage + 1;
		_notifications.Warning(
			$"Loading page {nextPage} will consume a Shodan query credit.",
			"Confirm pagination",
			() => _ = LoadPageAsync(nextPage),
			"Load next page");
	}

	/// <summary>Adds a filter chip from <see cref="NewFilterName" />/<see cref="NewFilterValue" />.</summary>
	[RelayCommand]
	private void AddFilterChip()
	{
		var name = NewFilterName?.Trim();
		var value = NewFilterValue?.Trim();
		if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
		{
			return;
		}

		if (!AppliedFilters.Any(chip =>
				string.Equals(chip.Name, name, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(chip.Value, value, StringComparison.Ordinal)))
		{
			AppliedFilters.Add(new FilterChip(name, value));
		}

		NewFilterName = null;
		NewFilterValue = null;
	}

	/// <summary>Removes a previously applied filter chip.</summary>
	[RelayCommand]
	private void RemoveFilterChip(FilterChip chip) => AppliedFilters.Remove(chip);

	/// <summary>Requests an additional facet breakdown from <see cref="NewFacetName" />.</summary>
	[RelayCommand]
	private void AddRequestedFacet()
	{
		var name = NewFacetName?.Trim();
		if (string.IsNullOrEmpty(name) || RequestedFacets.Contains(name, StringComparer.OrdinalIgnoreCase))
		{
			return;
		}

		RequestedFacets.Add(name);
		NewFacetName = null;
	}

	/// <summary>Removes a previously requested facet.</summary>
	[RelayCommand]
	private void RemoveRequestedFacet(string facet) => RequestedFacets.Remove(facet);

	/// <summary>Adds a filter chip from a clicked facet bucket and re-runs the search.</summary>
	[RelayCommand]
	private async Task UseFacetValueAsync(FacetValueRow row)
	{
		if (!AppliedFilters.Any(chip =>
				string.Equals(chip.Name, row.FacetName, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(chip.Value, row.Value, StringComparison.Ordinal)))
		{
			AppliedFilters.Add(new FilterChip(row.FacetName, row.Value));
		}

		IsRawQueryMode = false;

		// Drilling into a facet bucket always re-runs SearchAsync with a filter present, which spends
		// a query credit - surface that up front (non-blocking; unlike LoadNextPage's confirm dialog,
		// gating every facet click behind a second click would make the core drill-down loop clunky).
		_notifications.Warning(
			$"Filtering by {row.FacetName}:{row.Value} and re-running the search — this uses 1 query credit.",
			"Facet filter");
		await SearchCommand.ExecuteAsync(null).ConfigureAwait(true);
	}

	/// <summary>Closes the drill-down detail panel.</summary>
	[RelayCommand]
	private void CloseBannerDetail() => SelectedBannerForDetail = null;

	/// <inheritdoc />
	protected override void OnValidationError(ShodanRequestValidationException ex) => QueryValidationError = ex.Message;

	private async Task LoadPageAsync(int page)
	{
		await RunAsync(async ct =>
		{
			var client = Accessor.Client;
			var query = EffectiveQuery;
			if (client is null || string.IsNullOrWhiteSpace(query))
			{
				return;
			}

			var result = await client.Search.SearchAsync(query, BuildFacetsParameter(), page, ct).ConfigureAwait(true);
			CurrentPage = page;
			SelectedBannerForDetail = null;
			ApplyResult(result);
			await _accountSession.RefreshAsync(true, ct).ConfigureAwait(true);
		}).ConfigureAwait(true);
	}

	private async Task LoadSuggestionsAsync()
	{
		await RunAsync(async ct =>
		{
			var client = Accessor.Client;
			if (client is null)
			{
				return;
			}

			var filtersTask = client.Search.GetFiltersAsync(ct);
			var facetsTask = client.Search.GetFacetsAsync(ct);
			await Task.WhenAll(filtersTask, facetsTask).ConfigureAwait(true);

			FilterNameSuggestions.Clear();
			foreach (var name in filtersTask.Result.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
			{
				FilterNameSuggestions.Add(name);
			}

			FacetNameSuggestions.Clear();
			foreach (var name in facetsTask.Result.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
			{
				FacetNameSuggestions.Add(name);
			}
		}).ConfigureAwait(true);
	}

	private void ApplyResult(SearchResult result)
	{
		CurrentResult = result;

		Matches.Clear();
		foreach (var banner in result.Matches)
		{
			Matches.Add(banner);
		}

		FacetGroups.Clear();
		foreach (var (name, values) in result.Facets)
		{
			var rows = values.Select(item => new FacetValueRow(name, item.Value, item.Count)).ToList();
			FacetGroups.Add(new FacetGroupDisplay(name, rows));
		}
	}

	private string? BuildFacetsParameter() => RequestedFacets.Count > 0 ? string.Join(',', RequestedFacets) : null;

	private string ComposeChipQuery()
	{
		var parts = new List<string>();
		if (!string.IsNullOrWhiteSpace(FreeTextTerm))
		{
			parts.Add(FreeTextTerm.Trim());
		}

		parts.AddRange(AppliedFilters.Select(chip => chip.QueryFragment));
		return string.Join(' ', parts);
	}

	private void OnEffectiveQueryInputsChanged()
	{
		QueryValidationError = null;
		OnPropertyChanged(nameof(EffectiveQuery));
		ScheduleParseValidation();
	}

	private void ScheduleParseValidation()
	{
		_parseDebounceCts?.Cancel();
		_parseDebounceCts?.Dispose();
		var cts = new CancellationTokenSource();
		_parseDebounceCts = cts;
		_ = RunDebouncedParseAsync(cts.Token);
	}

	private async Task RunDebouncedParseAsync(CancellationToken debounceToken)
	{
		try
		{
			await Task.Delay(ParseDebounceDelay, debounceToken).ConfigureAwait(true);
		}
		catch (OperationCanceledException)
		{
			return;
		}

		if (debounceToken.IsCancellationRequested)
		{
			return;
		}

		try
		{
			await RunAsync(async ct =>
			{
				var client = Accessor.Client;
				var query = EffectiveQuery;
				if (client is null || string.IsNullOrWhiteSpace(query))
				{
					LastParse = null;
					HasQueryErrors = false;
					return;
				}

				var parse = await client.Search.ParseQueryAsync(query, ct).ConfigureAwait(true);
				LastParse = parse;
				HasQueryErrors = parse.Errors.Count > 0;
			}, debounceToken).ConfigureAwait(true);
		}
		catch (OperationCanceledException)
		{
			// Superseded by a newer keystroke; ignore.
		}
	}
}
