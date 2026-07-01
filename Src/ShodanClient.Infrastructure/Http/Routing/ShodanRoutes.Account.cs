namespace ShodanClient.Infrastructure.Http.Routing;

internal static partial class ShodanRoutes
{
	/// <summary>Routes for the Account family of endpoints on the REST API.</summary>
	public static class Account
	{
		public static ShodanRoute Profile() =>
			new(ShodanApiSurface.Rest, HttpMethod.Get, "account/profile");
	}
}
