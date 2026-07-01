using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Controls;
using ShodanClient.App.Services.AccountSession;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.App.ViewModels.Search;
using ShodanClient.Application.Exceptions;
using ShodanClient.Domain.Trends;

namespace ShodanClient.App.ViewModels.Trends;

/// <summary>
///     Historical, month-to-month search trends. Reuses <see cref="KnownFilters" /> from the Search
///     module for quick-insert hints, but keeps its own light query/facets input rather than a full
///     chip editor — Trends filters/facets are discovered from <c>ITrendsService</c>, a separate
///     endpoint from <c>ISearchService</c>.
/// </summary>
public sealed partial class TrendsViewModel : ModuleViewModelBase
{
	private readonly IAccountSessionService _accountSession;

	/// <summary>Creates the Trends module view model.</summary>
	public TrendsViewModel(INotificationService notifications, IShodanClientAccessor accessor,
		IAccountSessionService accountSession)
		: base(notifications)
	{
		Accessor = accessor;
		_accountSession = accountSession;
		Title = "Trends";

		_ = LoadSuggestionsAsync();
	}

	/// <summary>Pending facet name for the "request a facet" row.</summary>
	[ObservableProperty]
	public partial string? NewFacetName { get; set; }

	/// <summary>The overall match-count trend, ready for the shared <see cref="TrendChart" />.</summary>
	[ObservableProperty]
	public partial IReadOnlyList<TrendPoint> OverallTrend { get; set; } = [];

	/// <summary>The Shodan search query to trend.</summary>
	[ObservableProperty]
	public partial string? Query { get; set; }

	/// <summary>A local, non-toast validation message (e.g. an empty query) shown next to the Search button.</summary>
	[ObservableProperty]
	public partial string? QueryValidationError { get; set; }

	/// <summary>The most recent Trends search result.</summary>
	[ObservableProperty]
	public partial TrendResult? Result { get; set; }

	/// <summary>The active Shodan client accessor.</summary>
	public IShodanClientAccessor Accessor { get; }

	/// <summary>A handful of curated filters, offered as quick-insert hints below the query box.</summary>
	public IReadOnlyList<KnownFilterInfo> HighlightedFilters { get; } = KnownFilters.Ordered.Take(6).ToArray();

	/// <summary>Filter names known to the Trends endpoint, for autocomplete.</summary>
	public ObservableCollection<string> FilterNameSuggestions { get; } = [];

	/// <summary>Facet names known to the Trends endpoint, for autocomplete.</summary>
	public ObservableCollection<string> FacetNameSuggestions { get; } = [];

	/// <summary>Facet names requested on the next search, summarized month by month alongside the trend.</summary>
	public ObservableCollection<string> Facets { get; } = [];

	/// <summary>Per-facet, per-value breakdown series (top 5 values per requested facet).</summary>
	public ObservableCollection<TrendFacetValueSeries> FacetSeries { get; } = [];

	/// <summary>Runs a Trends search for <see cref="Query" />, summarizing <see cref="Facets" /> month by month.</summary>
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

			var query = Query?.Trim();
			if (string.IsNullOrWhiteSpace(query))
			{
				throw new ShodanRequestValidationException("Enter a search query before running a Trends search.");
			}

			QueryValidationError = null;
			var facets = Facets.Count > 0 ? string.Join(',', Facets) : null;
			var result = await client.Trends.SearchAsync(query, facets, ct).ConfigureAwait(true);
			Result = result;
			RebuildOverallTrend(result);
			RebuildFacetSeries(result);

			// Trends' search endpoint always spends a query credit (there is no free count/facet-only
			// call here, unlike Search's CountAsync), so force the credit display to refresh now
			// rather than silently coalescing within AccountSessionService's 10-second window.
			await _accountSession.RefreshAsync(true, ct).ConfigureAwait(true);
		}).ConfigureAwait(true);
	}

	/// <summary>Requests an additional facet breakdown from <see cref="NewFacetName" />.</summary>
	[RelayCommand]
	private void AddFacet()
	{
		var name = NewFacetName?.Trim();
		if (string.IsNullOrEmpty(name) || Facets.Contains(name, StringComparer.OrdinalIgnoreCase))
		{
			return;
		}

		Facets.Add(name);
		NewFacetName = null;
	}

	/// <summary>Removes a previously requested facet.</summary>
	[RelayCommand]
	private void RemoveFacet(string facet) => Facets.Remove(facet);

	/// <summary>Appends a curated filter's name to <see cref="Query" /> as a starting point.</summary>
	[RelayCommand]
	private void InsertKnownFilter(KnownFilterInfo info)
	{
		var prefix = string.IsNullOrWhiteSpace(Query) ? string.Empty : $"{Query.TrimEnd()} ";
		Query = $"{prefix}{info.Name}:";
	}

	/// <inheritdoc />
	protected override void OnValidationError(ShodanRequestValidationException ex) => QueryValidationError = ex.Message;

	private async Task LoadSuggestionsAsync()
	{
		await RunAsync(async ct =>
		{
			var client = Accessor.Client;
			if (client is null)
			{
				return;
			}

			var filtersTask = client.Trends.GetFiltersAsync(ct);
			var facetsTask = client.Trends.GetFacetsAsync(ct);
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

	private void RebuildOverallTrend(TrendResult result) =>
		// Reassigning (rather than mutating the previous list/collection in place) is required here:
		// TrendChart only redraws when its ItemsSourceProperty itself changes (or its Bounds do), and
		// Avalonia only re-evaluates this binding when OverallTrend's own PropertyChanged fires - an
		// in-place Clear()/Add() on a long-lived ObservableCollection does neither, so the chart and
		// the section's IsVisible would otherwise never update after the first (empty) render.
		OverallTrend = result.Matches.Select(match => new TrendPoint(match.Month, match.Count)).ToList();

	private void RebuildFacetSeries(TrendResult result)
	{
		FacetSeries.Clear();

		foreach (var (facetName, monthGroups) in result.Facets)
		{
			var totals = new Dictionary<string, long>(StringComparer.Ordinal);
			foreach (var group in monthGroups)
			{
				foreach (var item in group.Values)
				{
					totals[item.Value] = totals.GetValueOrDefault(item.Value) + item.Count;
				}
			}

			var topValues = totals
				.OrderByDescending(kvp => kvp.Value)
				.Take(5)
				.Select(kvp => kvp.Key);

			foreach (var value in topValues)
			{
				var points = monthGroups
					.Select(group =>
						new TrendPoint(group.Key, group.Values.FirstOrDefault(item => item.Value == value).Count))
					.ToList();
				FacetSeries.Add(new TrendFacetValueSeries(facetName, value, points));
			}
		}
	}
}
