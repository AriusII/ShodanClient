using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Alerts;
using ShodanClient.Domain.Alerts;
using ShodanClient.Infrastructure.Alerts.Mapping;
using ShodanClient.Infrastructure.Alerts.Wire;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Alerts;

/// <summary>REST implementation of <see cref="IAlertRepository" />.</summary>
internal sealed class AlertRepository(RestChannel channel) : IAlertRepository
{
	public async Task<Alert> CreateAsync(CreateAlertQuery query, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Alerts.Create();
		var payload = new CreateAlertPayload
		{
			Name = query.Name,
			Filters = new AlertFilterPayload { Ip = [.. query.Ips] },
			Expires = query.Expires
		};
		var response = await channel
			.SendJsonAsync(
				route,
				payload,
				ShodanJsonContext.Default.CreateAlertPayload,
				ShodanJsonContext.Default.AlertDto,
				cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<IReadOnlyList<Alert>> ListAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Alerts.List();
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.AlertDtoArray, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<Alert> GetAsync(string id, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Alerts.Get(id);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.AlertDto, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<Alert> UpdateNetworksAsync(UpdateAlertNetworksQuery query, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Alerts.Update(query.Id);
		var payload = new UpdateAlertPayload
		{
			Filters = new AlertFilterPayload { Ip = [.. query.Ips] }
		};
		var response = await channel
			.SendJsonAsync(
				route,
				payload,
				ShodanJsonContext.Default.UpdateAlertPayload,
				ShodanJsonContext.Default.AlertDto,
				cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Alerts.Delete(id);
		return await channel.SendForSuccessAsync(route, null, cancellationToken).ConfigureAwait(false);
	}

	public async Task<IReadOnlyList<TriggerDefinition>> GetTriggersAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Alerts.Triggers();
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.TriggerDefinitionDtoArray, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<bool> EnableTriggerAsync(string id, string trigger, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Alerts.EnableTrigger(id, trigger);
		return await channel.SendForSuccessAsync(route, null, cancellationToken).ConfigureAwait(false);
	}

	public async Task<bool> DisableTriggerAsync(string id, string trigger, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Alerts.DisableTrigger(id, trigger);
		return await channel.SendForSuccessAsync(route, null, cancellationToken).ConfigureAwait(false);
	}

	public async Task<bool> IgnoreTriggerServiceAsync(
		string id,
		string trigger,
		string service,
		CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Alerts.IgnoreTriggerService(id, trigger, service);
		return await channel.SendForSuccessAsync(route, null, cancellationToken).ConfigureAwait(false);
	}

	public async Task<bool> UnignoreTriggerServiceAsync(
		string id,
		string trigger,
		string service,
		CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Alerts.UnignoreTriggerService(id, trigger, service);
		return await channel.SendForSuccessAsync(route, null, cancellationToken).ConfigureAwait(false);
	}

	public async Task<bool> AddNotifierAsync(string id, string notifierId, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Alerts.AddNotifier(id, notifierId);
		return await channel.SendForSuccessAsync(route, null, cancellationToken).ConfigureAwait(false);
	}

	public async Task<bool> RemoveNotifierAsync(string id, string notifierId, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Alerts.RemoveNotifier(id, notifierId);
		return await channel.SendForSuccessAsync(route, null, cancellationToken).ConfigureAwait(false);
	}
}
