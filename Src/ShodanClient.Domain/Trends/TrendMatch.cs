namespace ShodanClient.Domain.Trends;

/// <summary>
///     A single month's match count from a Trends search — one element of a
///     <see cref="TrendResult" />'s <c>matches[]</c> array.
/// </summary>
/// <param name="Month">The month the count applies to, formatted <c>YYYY-MM</c> (<c>month</c>).</param>
/// <param name="Count">The number of matches for that month (<c>count</c>).</param>
public readonly record struct TrendMatch(string Month, long Count);
