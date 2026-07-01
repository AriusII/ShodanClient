namespace ShodanClient.Infrastructure.Http.Routing;

internal static partial class ShodanRoutes
{
	/// <summary>Routes for the Directory family of endpoints on the REST API.</summary>
	public static class Directory
	{
		public static ShodanRoute ListQueries(int page, string? sort, string? order)
		{
			var query = new QueryStringBuilder(48);
			if (page > 1)
			{
				query.Add("page", page);
			}

			query.AddIfPresent("sort", sort);
			query.AddIfPresent("order", order);
			return new ShodanRoute(ShodanApiSurface.Rest, HttpMethod.Get, $"shodan/query{query.Build()}");
		}

		public static ShodanRoute SearchQueries(string searchQuery, int page)
		{
			var query = new QueryStringBuilder(searchQuery.Length + 32);
			query.Add("query", searchQuery);
			if (page > 1)
			{
				query.Add("page", page);
			}

			return new ShodanRoute(ShodanApiSurface.Rest, HttpMethod.Get, $"shodan/query/search{query.Build()}");
		}

		public static ShodanRoute Tags(int size)
		{
			var query = new QueryStringBuilder(16);
			query.Add("size", size);
			return new ShodanRoute(ShodanApiSurface.Rest, HttpMethod.Get, $"shodan/query/tags{query.Build()}");
		}
	}
}
