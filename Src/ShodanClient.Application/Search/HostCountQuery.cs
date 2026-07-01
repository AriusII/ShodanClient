using ShodanClient.Application.Common;

namespace ShodanClient.Application.Search;

/// <summary>Parameters for a count-only search (<c>GET /shodan/host/count</c>).</summary>
/// <param name="Query">The Shodan search query.</param>
/// <param name="Facets">Optional comma-separated list of facets to summarize.</param>
internal sealed record HostCountQuery(string Query, string? Facets = null) : IShodanQuery;
