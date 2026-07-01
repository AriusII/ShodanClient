namespace ShodanClient.Infrastructure.Http.Routing;

internal static partial class ShodanRoutes
{
	/// <summary>Routes for the Utility family of endpoints on the REST API.</summary>
	public static class Utility
	{
		public static ShodanRoute HttpHeaders() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "tools/httpheaders");

		public static ShodanRoute MyIp() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "tools/myip");
	}
}
