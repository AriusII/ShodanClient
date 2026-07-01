using ShodanClient.Application.Common;

namespace ShodanClient.Application.Directory;

/// <summary>Parameters for searching saved queries (<c>GET /shodan/query/search</c>).</summary>
/// <param name="Query">The text to search for in the directory of saved queries.</param>
/// <param name="Page">The 1-based page number.</param>
internal sealed record SearchSavedQueriesQuery(string Query, int Page = 1) : IShodanQuery;
