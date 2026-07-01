using ShodanClient.Application.Common;

namespace ShodanClient.Application.Scanning;

/// <summary>
///     Parameters for requesting an Internet-wide crawl of a port/protocol
///     (<c>POST /shodan/scan/internet</c>).
/// </summary>
/// <param name="Port">The port to scan.</param>
/// <param name="Protocol">The name of the protocol to use when scanning.</param>
internal sealed record ScanInternetQuery(int Port, string Protocol) : IShodanQuery;
