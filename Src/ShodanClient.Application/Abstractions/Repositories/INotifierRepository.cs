using ShodanClient.Application.Notifiers;
using ShodanClient.Domain.Notifiers;

namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to the Notifiers family of endpoints on the REST API.</summary>
internal interface INotifierRepository
{
	/// <summary>Lists the user's configured notifiers (<c>GET /notifier</c>).</summary>
	Task<IReadOnlyList<Notifier>> ListAsync(CancellationToken cancellationToken);

	/// <summary>Creates a new notifier (<c>POST /notifier</c>).</summary>
	Task<CreateNotifierResult> CreateAsync(CreateNotifierQuery query, CancellationToken cancellationToken);

	/// <summary>
	///     Lists the notification providers Shodan supports and the arguments each requires
	///     (<c>GET /notifier/provider</c>).
	/// </summary>
	Task<IReadOnlyList<NotifierProvider>> ListProvidersAsync(CancellationToken cancellationToken);

	/// <summary>Gets a single notifier by id (<c>GET /notifier/{id}</c>).</summary>
	Task<Notifier> GetAsync(string id, CancellationToken cancellationToken);

	/// <summary>Updates a notifier's arguments (<c>PUT /notifier/{id}</c>).</summary>
	Task<bool> UpdateAsync(UpdateNotifierQuery query, CancellationToken cancellationToken);

	/// <summary>Deletes a notifier (<c>DELETE /notifier/{id}</c>).</summary>
	Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}
