using ShodanClient.Domain.Alerts;

namespace ShodanClient.Application.Abstractions.Services;

/// <summary>
///     Creating and managing network alerts — monitored IP ranges, the triggers that fire
///     notifications, and the notifiers attached to them. Exposed on the client as
///     <c>IShodanClient.Alerts</c>.
/// </summary>
public interface IAlertService
{
	/// <summary>Creates a network alert (<c>POST /shodan/alert</c>).</summary>
	/// <param name="name">A display name describing the alert.</param>
	/// <param name="ips">The IPs or network ranges (CIDR notation) to monitor.</param>
	/// <param name="expiresSeconds">
	///     The number of seconds the alert stays active, or <c>0</c> (the default) for it to never expire.
	/// </param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<Alert> CreateAsync(
		string name,
		IEnumerable<string> ips,
		int expiresSeconds = 0,
		CancellationToken cancellationToken = default);

	/// <summary>Lists every alert configured on the account (<c>GET /shodan/alert/info</c>).</summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<IReadOnlyList<Alert>> ListAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets a single alert by id (<c>GET /shodan/alert/{id}/info</c>).</summary>
	/// <param name="id">The alert identifier.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<Alert> GetAsync(string id, CancellationToken cancellationToken = default);

	/// <summary>Replaces the monitored networks on an alert (<c>POST /shodan/alert/{id}</c>).</summary>
	/// <param name="id">The alert identifier.</param>
	/// <param name="ips">The new set of IPs or network ranges (CIDR notation) to monitor.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<Alert> UpdateNetworksAsync(string id, IEnumerable<string> ips, CancellationToken cancellationToken = default);

	/// <summary>Deletes an alert (<c>DELETE /shodan/alert/{id}</c>).</summary>
	/// <param name="id">The alert identifier.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

	/// <summary>Lists the triggers that can be enabled on an alert (<c>GET /shodan/alert/triggers</c>).</summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<IReadOnlyList<TriggerDefinition>> GetTriggersAsync(CancellationToken cancellationToken = default);

	/// <summary>Enables a trigger on an alert (<c>PUT /shodan/alert/{id}/trigger/{trigger}</c>).</summary>
	/// <param name="id">The alert identifier.</param>
	/// <param name="trigger">The trigger name, e.g. <c>malware</c>, <c>vulnerable</c>.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<bool> EnableTriggerAsync(string id, string trigger, CancellationToken cancellationToken = default);

	/// <summary>Disables a trigger on an alert (<c>DELETE /shodan/alert/{id}/trigger/{trigger}</c>).</summary>
	/// <param name="id">The alert identifier.</param>
	/// <param name="trigger">The trigger name, e.g. <c>malware</c>, <c>vulnerable</c>.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<bool> DisableTriggerAsync(string id, string trigger, CancellationToken cancellationToken = default);

	/// <summary>
	///     Whitelists a service (<c>ip:port</c>) so it no longer fires a trigger
	///     (<c>PUT /shodan/alert/{id}/trigger/{trigger}/ignore/{service}</c>).
	/// </summary>
	/// <param name="id">The alert identifier.</param>
	/// <param name="trigger">The trigger name.</param>
	/// <param name="service">The service to whitelist, formatted as <c>ip:port</c>.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<bool> IgnoreTriggerServiceAsync(
		string id,
		string trigger,
		string service,
		CancellationToken cancellationToken = default);

	/// <summary>
	///     Removes a service from a trigger's whitelist
	///     (<c>DELETE /shodan/alert/{id}/trigger/{trigger}/ignore/{service}</c>).
	/// </summary>
	/// <param name="id">The alert identifier.</param>
	/// <param name="trigger">The trigger name.</param>
	/// <param name="service">The whitelisted service, formatted as <c>ip:port</c>.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<bool> UnignoreTriggerServiceAsync(
		string id,
		string trigger,
		string service,
		CancellationToken cancellationToken = default);

	/// <summary>Attaches a notifier to an alert (<c>PUT /shodan/alert/{id}/notifier/{notifierId}</c>).</summary>
	/// <param name="id">The alert identifier.</param>
	/// <param name="notifierId">The notifier identifier to attach.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<bool> AddNotifierAsync(string id, string notifierId, CancellationToken cancellationToken = default);

	/// <summary>Detaches a notifier from an alert (<c>DELETE /shodan/alert/{id}/notifier/{notifierId}</c>).</summary>
	/// <param name="id">The alert identifier.</param>
	/// <param name="notifierId">The notifier identifier to detach.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<bool> RemoveNotifierAsync(string id, string notifierId, CancellationToken cancellationToken = default);
}
