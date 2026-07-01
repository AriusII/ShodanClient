using ShodanClient.Application.Common;

namespace ShodanClient.Application.Search;

/// <summary>Parameters for a Shodan search (<c>GET /shodan/host/search</c>).</summary>
/// <param name="Query">The Shodan search query.</param>
/// <param name="Facets">Optional comma-separated list of facets to summarize.</param>
/// <param name="Page">The 1-based page number (100 results per page).</param>
internal sealed record HostSearchQuery(string Query, string? Facets = null, int Page = 1) : IShodanQuery;
