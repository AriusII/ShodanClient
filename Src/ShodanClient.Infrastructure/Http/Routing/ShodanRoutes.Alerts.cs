namespace ShodanClient.Infrastructure.Http.Routing;

internal static partial class ShodanRoutes
{
	/// <summary>Routes for the Alerts (network alert) family of endpoints on the REST API.</summary>
	public static class Alerts
	{
		public static ShodanRoute Create() =>
			new(ShodanApiSurface.Rest, HttpMethod.Post, "shodan/alert");

		public static ShodanRoute List() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "shodan/alert/info");

		public static ShodanRoute Get(string id) =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, $"shodan/alert/{Uri.EscapeDataString(id)}/info");

		public static ShodanRoute Update(string id) =>
			new(ShodanApiSurface.Rest, HttpMethod.Post, $"shodan/alert/{Uri.EscapeDataString(id)}");

		public static ShodanRoute Delete(string id) =>
			new(ShodanApiSurface.Rest, HttpMethod.Delete, $"shodan/alert/{Uri.EscapeDataString(id)}");

		public static ShodanRoute Triggers() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "shodan/alert/triggers");

		public static ShodanRoute EnableTrigger(string id, string trigger) =>
			new(
				ShodanApiSurface.Rest,
				HttpMethod.Put,
				$"shodan/alert/{Uri.EscapeDataString(id)}/trigger/{Uri.EscapeDataString(trigger)}");

		public static ShodanRoute DisableTrigger(string id, string trigger) =>
			new(
				ShodanApiSurface.Rest,
				HttpMethod.Delete,
				$"shodan/alert/{Uri.EscapeDataString(id)}/trigger/{Uri.EscapeDataString(trigger)}");

		public static ShodanRoute IgnoreTriggerService(string id, string trigger, string service) =>
			new(
				ShodanApiSurface.Rest,
				HttpMethod.Put,
				$"shodan/alert/{Uri.EscapeDataString(id)}/trigger/{Uri.EscapeDataString(trigger)}/ignore/{Uri.EscapeDataString(service)}");

		public static ShodanRoute UnignoreTriggerService(string id, string trigger, string service) =>
			new(
				ShodanApiSurface.Rest,
				HttpMethod.Delete,
				$"shodan/alert/{Uri.EscapeDataString(id)}/trigger/{Uri.EscapeDataString(trigger)}/ignore/{Uri.EscapeDataString(service)}");

		public static ShodanRoute AddNotifier(string id, string notifierId) =>
			new(
				ShodanApiSurface.Rest,
				HttpMethod.Put,
				$"shodan/alert/{Uri.EscapeDataString(id)}/notifier/{Uri.EscapeDataString(notifierId)}");

		public static ShodanRoute RemoveNotifier(string id, string notifierId) =>
			new(
				ShodanApiSurface.Rest,
				HttpMethod.Delete,
				$"shodan/alert/{Uri.EscapeDataString(id)}/notifier/{Uri.EscapeDataString(notifierId)}");
	}
}
