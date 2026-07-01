using ShodanClient.Application.Common;

namespace ShodanClient.Application.Directory;

/// <summary>Parameters for listing saved queries (<c>GET /shodan/query</c>).</summary>
/// <param name="Page">The 1-based page number.</param>
/// <param name="Sort">Optional field to sort on (e.g. <c>votes</c>, <c>timestamp</c>).</param>
/// <param name="Order">Optional sort order (<c>asc</c> or <c>desc</c>).</param>
internal sealed record ListSavedQueriesQuery(int Page = 1, string? Sort = null, string? Order = null) : IShodanQuery;
