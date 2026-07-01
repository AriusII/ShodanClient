namespace ShodanClient.Infrastructure.Http.Routing;

internal static partial class ShodanRoutes
{
	/// <summary>Routes for the API status family of endpoints on the REST API.</summary>
	public static class ApiStatus
	{
		public static ShodanRoute Info() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "api-info");
	}
}
