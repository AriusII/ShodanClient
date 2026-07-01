namespace ShodanClient.Infrastructure.Http.Routing;

/// <summary>
///     The central route registry. Each bounded context contributes a nested static class in its own
///     partial file (<c>ShodanRoutes.Search.cs</c>, <c>ShodanRoutes.Scanning.cs</c>, …) that returns
///     strongly-typed <see cref="ShodanRoute" /> values. This is the single, discoverable home for
///     every relative path the client can call.
/// </summary>
internal static partial class ShodanRoutes;
