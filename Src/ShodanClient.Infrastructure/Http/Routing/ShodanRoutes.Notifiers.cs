namespace ShodanClient.Infrastructure.Http.Routing;

internal static partial class ShodanRoutes
{
	/// <summary>Routes for the Notifiers family of endpoints on the REST API.</summary>
	public static class Notifiers
	{
		public static ShodanRoute List() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "notifier");

		public static ShodanRoute Create() =>
			new(ShodanApiSurface.Rest, HttpMethod.Post, "notifier");

		public static ShodanRoute Providers() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "notifier/provider");

		public static ShodanRoute Get(string id) =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, $"notifier/{Uri.EscapeDataString(id)}");

		public static ShodanRoute Update(string id) =>
			new(ShodanApiSurface.Rest, HttpMethod.Put, $"notifier/{Uri.EscapeDataString(id)}");

		public static ShodanRoute Delete(string id) =>
			new(ShodanApiSurface.Rest, HttpMethod.Delete, $"notifier/{Uri.EscapeDataString(id)}");
	}
}
