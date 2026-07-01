using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Abstractions.Services;
using ShodanClient.Application.Common;
using ShodanClient.Application.Notifiers;
using ShodanClient.Domain.Notifiers;

namespace ShodanClient.Infrastructure.Notifiers;

/// <summary>
///     Logic layer for managing notifiers. Validates inputs and delegates to the repository.
/// </summary>
internal sealed class NotifierService(INotifierRepository repository) : INotifierService
{
	public Task<IReadOnlyList<Notifier>> ListAsync(CancellationToken cancellationToken = default) =>
		repository.ListAsync(cancellationToken);

	public Task<CreateNotifierResult> CreateAsync(
		string provider,
		string? description,
		IReadOnlyDictionary<string, string> arguments,
		CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(provider);
		Guard.NotNullOrEmpty(arguments);
		return repository.CreateAsync(new CreateNotifierQuery(provider, description, arguments), cancellationToken);
	}

	public Task<IReadOnlyList<NotifierProvider>> GetProvidersAsync(CancellationToken cancellationToken = default) =>
		repository.ListProvidersAsync(cancellationToken);

	public Task<Notifier> GetAsync(string id, CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(id);
		return repository.GetAsync(id, cancellationToken);
	}

	public Task<bool> UpdateAsync(
		string id,
		IReadOnlyDictionary<string, string> arguments,
		CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(id);
		Guard.NotNullOrEmpty(arguments);
		return repository.UpdateAsync(new UpdateNotifierQuery(id, arguments), cancellationToken);
	}

	public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
	{
		Guard.NotNullOrWhiteSpace(id);
		return repository.DeleteAsync(id, cancellationToken);
	}
}
