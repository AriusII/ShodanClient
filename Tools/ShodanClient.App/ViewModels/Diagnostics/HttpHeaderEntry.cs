namespace ShodanClient.App.ViewModels.Diagnostics;

/// <summary>A single HTTP header name/value pair, as seen by Shodan (<c>GET /tools/httpheaders</c>).</summary>
public sealed record HttpHeaderEntry(string Name, string Value);
