namespace ShodanClient.App.ViewModels.Scans;

/// <summary>
///     A single crawlable protocol as returned by <c>IScanService.GetProtocolsAsync</c>, paired with
///     its human-readable description for display in the reference panel and the Scan Internet
///     protocol picker.
/// </summary>
/// <param name="Name">The protocol's identifier, e.g. <c>http</c>.</param>
/// <param name="Description">A short human-readable description of the protocol.</param>
public readonly record struct ProtocolInfo(string Name, string Description);
