namespace ShodanClient.App.ViewModels.Dns;

/// <summary>A single row of the Resolve tab's result table (<c>IDnsService.ResolveAsync</c>).</summary>
/// <param name="Hostname">The hostname that was resolved.</param>
/// <param name="IpAddress">The resolved IP address, or <see langword="null" /> if resolution failed.</param>
public readonly record struct HostnameResolution(string Hostname, string? IpAddress);

/// <summary>A single row of the Reverse tab's result table (<c>IDnsService.ReverseAsync</c>).</summary>
/// <param name="IpAddress">The IP address that was reverse-resolved.</param>
/// <param name="Hostnames">The hostnames pointing at <paramref name="IpAddress" />, empty if none.</param>
public readonly record struct IpReverseResolution(string IpAddress, IReadOnlyList<string> Hostnames);
