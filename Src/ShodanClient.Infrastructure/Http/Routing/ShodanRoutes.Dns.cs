namespace ShodanClient.Infrastructure.Http.Routing;

internal static partial class ShodanRoutes
{
	/// <summary>Routes for the DNS family of endpoints on the REST API.</summary>
	public static class Dns
	{
		public static ShodanRoute Domain(string domain, bool history = false, string? type = null, int page = 1)
		{
			var query = new QueryStringBuilder(32);
			query.AddFlag("history", history);
			query.AddIfPresent("type", type);
			if (page > 1)
			{
				query.Add("page", page);
			}

			return new ShodanRoute(
				ShodanApiSurface.Rest,
				HttpMethod.Get,
				$"dns/domain/{Uri.EscapeDataString(domain)}{query.Build()}");
		}

		public static ShodanRoute Resolve(string hostnamesCsv)
		{
			var query = new QueryStringBuilder(hostnamesCsv.Length + 16);
			query.Add("hostnames", hostnamesCsv);
			return new ShodanRoute(ShodanApiSurface.Rest, HttpMethod.Get, $"dns/resolve{query.Build()}");
		}

		public static ShodanRoute Reverse(string ipsCsv)
		{
			var query = new QueryStringBuilder(ipsCsv.Length + 16);
			query.Add("ips", ipsCsv);
			return new ShodanRoute(ShodanApiSurface.Rest, HttpMethod.Get, $"dns/reverse{query.Build()}");
		}
	}
}
