using ShodanClient.Application.Common;

namespace ShodanClient.Application.Notifiers;

/// <summary>Parameters for creating a notifier (<c>POST /notifier</c>).</summary>
/// <param name="Provider">The notification provider to use, e.g. <c>email</c>, <c>slack</c>.</param>
/// <param name="Description">An optional human-readable description of the notifier.</param>
/// <param name="Arguments">Provider-specific arguments, e.g. <c>{ "to": "..." }</c>.</param>
internal sealed record CreateNotifierQuery(
	string Provider,
	string? Description,
	IReadOnlyDictionary<string, string> Arguments) : IShodanQuery;
