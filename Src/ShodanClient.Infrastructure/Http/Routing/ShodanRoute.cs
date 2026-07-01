namespace ShodanClient.Infrastructure.Http.Routing;

/// <summary>
///     A single API operation expressed as an owning surface, an HTTP verb and a base-relative path
///     (including any query string). A route NEVER contains the base URL (resolved from
///     <see cref="ShodanSurfaceRegistry" />) nor the API key (appended by the authentication handler),
///     which keeps the base-URL ⇄ route separation clean and the routes reusable.
/// </summary>
/// <param name="Surface">The surface that owns this route.</param>
/// <param name="Method">The HTTP method.</param>
/// <param name="RelativePath">The base-relative path and query, e.g. <c>shodan/host/1.1.1.1?minify=true</c>.</param>
internal readonly record struct ShodanRoute(ShodanApiSurface Surface, HttpMethod Method, string RelativePath);
