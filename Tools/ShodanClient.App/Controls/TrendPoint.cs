namespace ShodanClient.App.Controls;

/// <summary>A single month-by-month data point plotted by <see cref="TrendChart" />.</summary>
public sealed record TrendPoint(string Month, long Count);
