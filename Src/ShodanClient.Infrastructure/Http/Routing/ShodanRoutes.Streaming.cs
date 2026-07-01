using System.Globalization;

namespace ShodanClient.Infrastructure.Http.Routing;

internal static partial class ShodanRoutes
{
	/// <summary>Routes for the newline-delimited-JSON banner feeds on the Streaming API.</summary>
	public static class Streaming
	{
		public static ShodanRoute AllBanners() =>
			new(ShodanApiSurface.Streaming, HttpMethod.Get, "shodan/banners");

		public static ShodanRoute Asn(IEnumerable<string> asns) =>
			new(
				ShodanApiSurface.Streaming,
				HttpMethod.Get,
				$"shodan/asn/{Uri.EscapeDataString(string.Join(',', asns))}");

		public static ShodanRoute Countries(IEnumerable<string> countryCodes) =>
			new(
				ShodanApiSurface.Streaming,
				HttpMethod.Get,
				$"shodan/countries/{Uri.EscapeDataString(string.Join(',', countryCodes))}");

		public static ShodanRoute Ports(IEnumerable<int> ports)
		{
			var csv = string.Join(',', ports.Select(static p => p.ToString(CultureInfo.InvariantCulture)));
			return new ShodanRoute(ShodanApiSurface.Streaming, HttpMethod.Get,
				$"shodan/ports/{Uri.EscapeDataString(csv)}");
		}

		public static ShodanRoute Vulnerabilities(IEnumerable<string> cveIds) =>
			new(
				ShodanApiSurface.Streaming,
				HttpMethod.Get,
				$"shodan/vulns/{Uri.EscapeDataString(string.Join(',', cveIds))}");

		public static ShodanRoute Custom(string query)
		{
			var queryString = new QueryStringBuilder(query.Length + 16);
			queryString.Add("query", query);
			return new ShodanRoute(ShodanApiSurface.Streaming, HttpMethod.Get, $"shodan/custom{queryString.Build()}");
		}

		public static ShodanRoute AllAlerts() =>
			new(ShodanApiSurface.Streaming, HttpMethod.Get, "shodan/alert");

		public static ShodanRoute Alert(string id) =>
			new(ShodanApiSurface.Streaming, HttpMethod.Get, $"shodan/alert/{Uri.EscapeDataString(id)}");
	}
}
