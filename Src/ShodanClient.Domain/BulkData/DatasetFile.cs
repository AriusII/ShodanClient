namespace ShodanClient.Domain.BulkData;

/// <summary>
///     A single downloadable file within a bulk-data dataset (<c>GET /shodan/data/{dataset}</c>).
/// </summary>
public sealed record DatasetFile
{
	/// <summary>The file's name (<c>name</c>).</summary>
	public required string Name { get; init; }

	/// <summary>The file size in bytes (<c>size</c>).</summary>
	public long Size { get; init; }

	/// <summary>The URL the file can be downloaded from (<c>url</c>).</summary>
	public Uri? Url { get; init; }

	/// <summary>The SHA-1 checksum of the file's contents (<c>sha1</c>).</summary>
	public string? Sha1 { get; init; }

	/// <summary>When the file was generated (<c>timestamp</c>).</summary>
	public DateTimeOffset? Timestamp { get; init; }
}
