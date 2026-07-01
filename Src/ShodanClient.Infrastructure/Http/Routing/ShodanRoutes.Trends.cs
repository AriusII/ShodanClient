namespace ShodanClient.Infrastructure.Http.Routing;

internal static partial class ShodanRoutes
{
	/// <summary>Routes for the Trends API (<c>https://trends.shodan.io</c>).</summary>
	public static class Trends
	{
		public static ShodanRoute Search(string searchQuery, string? facets)
		{
			var query = new QueryStringBuilder(searchQuery.Length + 32);
			query.Add("query", searchQuery);
			query.AddIfPresent("facets", facets);
			return new ShodanRoute(ShodanApiSurface.Trends, HttpMethod.Get, $"api/v1/search{query.Build()}");
		}

		public static ShodanRoute Filters() =>
			new(ShodanApiSurface.Trends, HttpMethod.Get, "api/v1/search/filters");

		public static ShodanRoute Facets() =>
			new(ShodanApiSurface.Trends, HttpMethod.Get, "api/v1/search/facets");
	}
}
