namespace ShodanClient.Infrastructure.Http.Routing;

internal static partial class ShodanRoutes
{
	/// <summary>Routes for the Bulk Data family of endpoints on the REST API.</summary>
	public static class BulkData
	{
		public static ShodanRoute Datasets() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "shodan/data");

		public static ShodanRoute Files(string dataset) =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, $"shodan/data/{Uri.EscapeDataString(dataset)}");
	}
}
