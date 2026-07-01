namespace ShodanClient.Infrastructure.Http.Routing;

internal static partial class ShodanRoutes
{
	/// <summary>Routes for the on-demand Scanning family of endpoints on the REST API.</summary>
	public static class Scanning
	{
		public static ShodanRoute Ports() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "shodan/ports");

		public static ShodanRoute Protocols() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "shodan/protocols");

		public static ShodanRoute Scan() =>
			new(ShodanApiSurface.Rest, HttpMethod.Post, "shodan/scan");

		public static ShodanRoute ScanInternet() =>
			new(ShodanApiSurface.Rest, HttpMethod.Post, "shodan/scan/internet");

		public static ShodanRoute Scans() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "shodan/scans");

		public static ShodanRoute ScanStatus(string id) =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, $"shodan/scans/{Uri.EscapeDataString(id)}");
	}
}
