using ShodanClient.Application.Alerts;
using ShodanClient.Domain.Alerts;

namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to the Alerts (network alert) family of endpoints on the REST API.</summary>
internal interface IAlertRepository
{
	/// <summary>Creates a network alert (<c>POST /shodan/alert</c>).</summary>
	Task<Alert> CreateAsync(CreateAlertQuery query, CancellationToken cancellationToken);

	/// <summary>Lists every alert configured on the account (<c>GET /shodan/alert/info</c>).</summary>
	Task<IReadOnlyList<Alert>> ListAsync(CancellationToken cancellationToken);

	/// <summary>Gets a single alert by id (<c>GET /shodan/alert/{id}/info</c>).</summary>
	Task<Alert> GetAsync(string id, CancellationToken cancellationToken);

	/// <summary>Replaces the monitored networks on an alert (<c>POST /shodan/alert/{id}</c>).</summary>
	Task<Alert> UpdateNetworksAsync(UpdateAlertNetworksQuery query, CancellationToken cancellationToken);

	/// <summary>Deletes an alert (<c>DELETE /shodan/alert/{id}</c>).</summary>
	Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);

	/// <summary>Lists the triggers that can be enabled on an alert (<c>GET /shodan/alert/triggers</c>).</summary>
	Task<IReadOnlyList<TriggerDefinition>> GetTriggersAsync(CancellationToken cancellationToken);

	/// <summary>Enables a trigger on an alert (<c>PUT /shodan/alert/{id}/trigger/{trigger}</c>).</summary>
	Task<bool> EnableTriggerAsync(string id, string trigger, CancellationToken cancellationToken);

	/// <summary>Disables a trigger on an alert (<c>DELETE /shodan/alert/{id}/trigger/{trigger}</c>).</summary>
	Task<bool> DisableTriggerAsync(string id, string trigger, CancellationToken cancellationToken);

	/// <summary>
	///     Whitelists a service (<c>ip:port</c>) from a trigger
	///     (<c>PUT /shodan/alert/{id}/trigger/{trigger}/ignore/{service}</c>).
	/// </summary>
	Task<bool> IgnoreTriggerServiceAsync(string id, string trigger, string service,
		CancellationToken cancellationToken);

	/// <summary>
	///     Removes a service from a trigger's whitelist
	///     (<c>DELETE /shodan/alert/{id}/trigger/{trigger}/ignore/{service}</c>).
	/// </summary>
	Task<bool> UnignoreTriggerServiceAsync(string id, string trigger, string service,
		CancellationToken cancellationToken);

	/// <summary>Attaches a notifier to an alert (<c>PUT /shodan/alert/{id}/notifier/{notifierId}</c>).</summary>
	Task<bool> AddNotifierAsync(string id, string notifierId, CancellationToken cancellationToken);

	/// <summary>Detaches a notifier from an alert (<c>DELETE /shodan/alert/{id}/notifier/{notifierId}</c>).</summary>
	Task<bool> RemoveNotifierAsync(string id, string notifierId, CancellationToken cancellationToken);
}
