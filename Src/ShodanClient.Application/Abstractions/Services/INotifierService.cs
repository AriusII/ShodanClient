using ShodanClient.Domain.Notifiers;

namespace ShodanClient.Application.Abstractions.Services;

/// <summary>
///     Managing notification channels that alerts can push matches to. Exposed on the client as
///     <c>IShodanClient.Notifiers</c>.
/// </summary>
public interface INotifierService
{
	/// <summary>Lists the user's configured notifiers (<c>GET /notifier</c>).</summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<IReadOnlyList<Notifier>> ListAsync(CancellationToken cancellationToken = default);

	/// <summary>Creates a new notifier (<c>POST /notifier</c>).</summary>
	/// <param name="provider">
	///     The notification provider to use, e.g. <c>email</c>, <c>slack</c>, <c>telegram</c>, <c>webhook</c>.
	/// </param>
	/// <param name="description">An optional human-readable description of the notifier.</param>
	/// <param name="arguments">Provider-specific arguments, e.g. <c>{ "to": "..." }</c>.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<CreateNotifierResult> CreateAsync(
		string provider,
		string? description,
		IReadOnlyDictionary<string, string> arguments,
		CancellationToken cancellationToken = default);

	/// <summary>
	///     Lists the notification providers Shodan supports and the arguments each requires
	///     (<c>GET /notifier/provider</c>).
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<IReadOnlyList<NotifierProvider>> GetProvidersAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets a single notifier by id (<c>GET /notifier/{id}</c>).</summary>
	/// <param name="id">The notifier identifier.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<Notifier> GetAsync(string id, CancellationToken cancellationToken = default);

	/// <summary>Updates a notifier's arguments (<c>PUT /notifier/{id}</c>).</summary>
	/// <param name="id">The notifier identifier.</param>
	/// <param name="arguments">The provider-specific arguments to set, e.g. <c>{ "to": "..." }</c>.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<bool> UpdateAsync(
		string id,
		IReadOnlyDictionary<string, string> arguments,
		CancellationToken cancellationToken = default);

	/// <summary>Deletes a notifier (<c>DELETE /notifier/{id}</c>).</summary>
	/// <param name="id">The notifier identifier.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
