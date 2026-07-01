namespace ShodanClient.App.ViewModels.Search;

/// <summary>One clickable facet-value row rendered in the results' facets side panel.</summary>
/// <param name="FacetName">The owning facet's name, e.g. <c>country</c>.</param>
/// <param name="Value">The bucket's value, e.g. <c>US</c>.</param>
/// <param name="Count">How many results share <paramref name="Value" />.</param>
public sealed record FacetValueRow(string FacetName, string Value, long Count);

/// <summary>A named group of <see cref="FacetValueRow" /> — one per facet requested on a search.</summary>
/// <param name="Name">The facet name, e.g. <c>country</c>.</param>
/// <param name="Values">The facet's buckets, in the order returned by the API.</param>
public sealed record FacetGroupDisplay(string Name, IReadOnlyList<FacetValueRow> Values);
