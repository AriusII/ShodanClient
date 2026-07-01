using System.Text.Json.Serialization;

namespace ShodanClient.Infrastructure.Serialization;

/// <summary>
///     Wire shape of the <c>{ "success": true }</c> acknowledgement returned by Shodan's
///     write operations (enabling triggers, attaching notifiers, editing members, …).
/// </summary>
internal sealed class ShodanSuccessResponse
{
	/// <summary>
	///     Whether the operation succeeded. Nullable because some write endpoints return a 2xx body
	///     that simply omits this field (e.g. an empty object, or the updated resource) rather than an
	///     explicit acknowledgement — <c>null</c> must be treated the same as "absent", not as "false".
	/// </summary>
	[JsonPropertyName("success")]
	public bool? Success { get; set; }
}
