using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Notifiers;
using ShodanClient.Domain.Notifiers;
using ShodanClient.Infrastructure.Http;
using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Notifiers.Mapping;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Notifiers;

/// <summary>REST implementation of <see cref="INotifierRepository" />.</summary>
internal sealed class NotifierRepository(RestChannel channel) : INotifierRepository
{
	public async Task<IReadOnlyList<Notifier>> ListAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Notifiers.List();
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.NotifierListResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<CreateNotifierResult> CreateAsync(CreateNotifierQuery query, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Notifiers.Create();
		var form = BuildForm(query.Provider, query.Description, query.Arguments);
		var response = await channel
			.SendFormAsync(route, form, ShodanJsonContext.Default.CreateNotifierResponse, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<IReadOnlyList<NotifierProvider>> ListProvidersAsync(CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Notifiers.Providers();
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.DictionaryStringNotifierProviderDto, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<Notifier> GetAsync(string id, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Notifiers.Get(id);
		var response = await channel
			.GetJsonAsync(route, ShodanJsonContext.Default.NotifierDto, cancellationToken)
			.ConfigureAwait(false);
		return response.ToDomain();
	}

	public async Task<bool> UpdateAsync(UpdateNotifierQuery query, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Notifiers.Update(query.Id);
		var form = BuildForm(null, null, query.Arguments);
		using var content = new FormUrlEncodedContent(form);
		return await channel.SendForSuccessAsync(route, content, cancellationToken).ConfigureAwait(false);
	}

	public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
	{
		var route = ShodanRoutes.Notifiers.Delete(id);
		return await channel.SendForSuccessAsync(route, null, cancellationToken).ConfigureAwait(false);
	}

	private static List<KeyValuePair<string, string>> BuildForm(
		string? provider,
		string? description,
		IReadOnlyDictionary<string, string> arguments)
	{
		var form = new List<KeyValuePair<string, string>>(arguments.Count + 2);
		if (!string.IsNullOrEmpty(provider))
		{
			form.Add(new KeyValuePair<string, string>("provider", provider));
		}

		if (!string.IsNullOrEmpty(description))
		{
			form.Add(new KeyValuePair<string, string>("description", description));
		}

		foreach (var (key, value) in arguments)
		{
			form.Add(new KeyValuePair<string, string>(key, value));
		}

		return form;
	}
}
