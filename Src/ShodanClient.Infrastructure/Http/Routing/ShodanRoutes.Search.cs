namespace ShodanClient.Infrastructure.Http.Routing;

internal static partial class ShodanRoutes
{
	/// <summary>Routes for the Search family of endpoints on the REST API.</summary>
	public static class Search
	{
		public static ShodanRoute Host(string ip, bool history, bool minify)
		{
			var query = new QueryStringBuilder(24);
			query.AddFlag("history", history);
			query.AddFlag("minify", minify);
			return new ShodanRoute(
				ShodanApiSurface.Rest,
				HttpMethod.Get,
				$"shodan/host/{Uri.EscapeDataString(ip)}{query.Build()}");
		}

		public static ShodanRoute SearchHosts(string searchQuery, string? facets, int page)
		{
			var query = new QueryStringBuilder(searchQuery.Length + 32);
			query.Add("query", searchQuery);
			query.AddIfPresent("facets", facets);
			if (page > 1)
			{
				query.Add("page", page);
			}

			return new ShodanRoute(ShodanApiSurface.Rest, HttpMethod.Get, $"shodan/host/search{query.Build()}");
		}

		public static ShodanRoute Count(string searchQuery, string? facets)
		{
			var query = new QueryStringBuilder(searchQuery.Length + 24);
			query.Add("query", searchQuery);
			query.AddIfPresent("facets", facets);
			return new ShodanRoute(ShodanApiSurface.Rest, HttpMethod.Get, $"shodan/host/count{query.Build()}");
		}

		public static ShodanRoute Facets() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "shodan/host/search/facets");

		public static ShodanRoute Filters() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "shodan/host/search/filters");

		public static ShodanRoute Tokens(string searchQuery)
		{
			var query = new QueryStringBuilder(searchQuery.Length + 16);
			query.Add("query", searchQuery);
			return new ShodanRoute(ShodanApiSurface.Rest, HttpMethod.Get, $"shodan/host/search/tokens{query.Build()}");
		}
	}
}
