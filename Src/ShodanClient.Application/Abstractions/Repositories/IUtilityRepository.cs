namespace ShodanClient.Application.Abstractions.Repositories;

/// <summary>Transport-level access to the utility family of endpoints on the REST API.</summary>
internal interface IUtilityRepository
{
	/// <summary>Returns the HTTP headers the client sends (<c>GET /tools/httpheaders</c>).</summary>
	Task<IReadOnlyDictionary<string, string>> GetHttpHeadersAsync(CancellationToken cancellationToken);

	/// <summary>Returns the caller's public IP address (<c>GET /tools/myip</c>).</summary>
	Task<string> GetMyIpAsync(CancellationToken cancellationToken);
}
