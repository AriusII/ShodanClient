using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Application.Alerts;
using ShodanClient.Application.Common;
using ShodanClient.Domain.Alerts;

namespace ShodanClient.Infrastructure.Alerts;

/// <summary>
///     Logic layer for managing network alerts, their triggers and notifier links. Validates inputs
///     and delegates to the repository.
/// </summary>
internal sealed class AlertService(IAlertRepository repository) : IAlertService
{
	public Task<Alert> CreateAsync(
		string name,
		IEnumerable<string> ips,
		int expiresSeconds = 0,
		CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(name);
		var targets = Guard.NotNullOrEmpty(ips as IReadOnlyList<string> ?? [.. ips]);
		Guard.AtLeast(expiresSeconds, 0);
		return repository.CreateAsync(new CreateAlertQuery(name, targets, expiresSeconds), cancellationToken);
	}

	public Task<IReadOnlyList<Alert>> ListAsync(CancellationToken cancellationToken = default) =>
		repository.ListAsync(cancellationToken);

	public Task<Alert> GetAsync(string id, CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(id);
		return repository.GetAsync(id, cancellationToken);
	}

	public Task<Alert> UpdateNetworksAsync(
		string id,
		IEnumerable<string> ips,
		CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(id);
		var targets = Guard.NotNullOrEmpty(ips as IReadOnlyList<string> ?? [.. ips]);
		return repository.UpdateNetworksAsync(new UpdateAlertNetworksQuery(id, targets), cancellationToken);
	}

	public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(id);
		return repository.DeleteAsync(id, cancellationToken);
	}

	public Task<IReadOnlyList<TriggerDefinition>> GetTriggersAsync(CancellationToken cancellationToken = default) =>
		repository.GetTriggersAsync(cancellationToken);

	public Task<bool> EnableTriggerAsync(string id, string trigger, CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(id);
		Guard.NotNullOrWhiteSpace(trigger);
		return repository.EnableTriggerAsync(id, trigger, cancellationToken);
	}

	public Task<bool> DisableTriggerAsync(string id, string trigger, CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(id);
		Guard.NotNullOrWhiteSpace(trigger);
		return repository.DisableTriggerAsync(id, trigger, cancellationToken);
	}

	public Task<bool> IgnoreTriggerServiceAsync(
		string id,
		string trigger,
		string service,
		CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(id);
		Guard.NotNullOrWhiteSpace(trigger);
		Guard.NotNullOrWhiteSpace(service);
		return repository.IgnoreTriggerServiceAsync(id, trigger, service, cancellationToken);
	}

	public Task<bool> UnignoreTriggerServiceAsync(
		string id,
		string trigger,
		string service,
		CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(id);
		Guard.NotNullOrWhiteSpace(trigger);
		Guard.NotNullOrWhiteSpace(service);
		return repository.UnignoreTriggerServiceAsync(id, trigger, service, cancellationToken);
	}

	public Task<bool> AddNotifierAsync(string id, string notifierId, CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(id);
		Guard.NotNullOrWhiteSpace(notifierId);
		return repository.AddNotifierAsync(id, notifierId, cancellationToken);
	}

	public Task<bool> RemoveNotifierAsync(string id, string notifierId, CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(id);
		Guard.NotNullOrWhiteSpace(notifierId);
		return repository.RemoveNotifierAsync(id, notifierId, cancellationToken);
	}
}
