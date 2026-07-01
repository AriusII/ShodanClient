using ShodanClient.Application.Common;

namespace ShodanClient.Application.Directory;

/// <summary>Parameters for listing popular saved-query tags (<c>GET /shodan/query/tags</c>).</summary>
/// <param name="Size">The number of tags to return.</param>
internal sealed record ListQueryTagsQuery(int Size = 10) : IShodanQuery;
