namespace ShodanClient.Application.Common;

/// <summary>
///     Marker implemented by request/query DTOs passed to the service layer. It carries no members;
///     it exists to give the query types a common, discoverable contract and a home for future
///     cross-cutting behavior (e.g. analyzers or generic validation helpers).
/// </summary>
internal interface IShodanQuery;
