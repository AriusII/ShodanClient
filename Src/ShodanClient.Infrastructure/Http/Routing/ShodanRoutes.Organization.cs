namespace ShodanClient.Infrastructure.Http.Routing;

internal static partial class ShodanRoutes
{
	/// <summary>Routes for the Organization-management family of endpoints on the REST API.</summary>
	public static class Organization
	{
		public static ShodanRoute Org() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "org");

		public static ShodanRoute AddMember(string user, bool notify)
		{
			var query = new QueryStringBuilder(16);
			query.AddFlag("notify", notify);
			return new ShodanRoute(
				ShodanApiSurface.Rest,
				HttpMethod.Put,
				$"org/member/{Uri.EscapeDataString(user)}{query.Build()}");
		}

		public static ShodanRoute RemoveMember(string user) =>
			new(ShodanApiSurface.Rest, HttpMethod.Delete, $"org/member/{Uri.EscapeDataString(user)}");
	}
}
