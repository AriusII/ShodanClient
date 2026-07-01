using ShodanClient.Application.Common;

namespace ShodanClient.Application.Trends;

/// <summary>Parameters for a Trends search (<c>GET /api/v1/search</c> on the Trends API).</summary>
/// <param name="Query">The Shodan search query.</param>
/// <param name="Facets">Optional comma-separated list of facets to summarize month by month.</param>
internal sealed record TrendSearchQuery(string Query, string? Facets = null) : IShodanQuery;
