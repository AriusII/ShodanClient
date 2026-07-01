using ShodanClient.Application.Common;

namespace ShodanClient.Application.Notifiers;

/// <summary>Parameters for updating a notifier's arguments (<c>PUT /notifier/{id}</c>).</summary>
/// <param name="Id">The identifier of the notifier to update.</param>
/// <param name="Arguments">The provider-specific arguments to set, e.g. <c>{ "to": "..." }</c>.</param>
internal sealed record UpdateNotifierQuery(string Id, IReadOnlyDictionary<string, string> Arguments) : IShodanQuery;
