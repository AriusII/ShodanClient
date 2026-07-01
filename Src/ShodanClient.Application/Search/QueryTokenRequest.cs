using ShodanClient.Application.Common;

namespace ShodanClient.Application.Search;

/// <summary>Parameters for parsing a query into tokens (<c>GET /shodan/host/search/tokens</c>).</summary>
/// <param name="Query">The Shodan search query to parse.</param>
internal sealed record QueryTokenRequest(string Query) : IShodanQuery;
