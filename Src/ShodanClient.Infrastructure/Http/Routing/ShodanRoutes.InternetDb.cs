namespace ShodanClient.Infrastructure.Http.Routing;

internal static partial class ShodanRoutes
{
	/// <summary>Routes for the key-less InternetDB API.</summary>
	public static class InternetDb
	{
		public static ShodanRoute Host(string ip) =>
			new(ShodanApiSurface.InternetDb, HttpMethod.Get, Uri.EscapeDataString(ip));
	}
}
