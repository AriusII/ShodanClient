using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace ShodanClient.Application.Configuration;

/// <summary>
///     Root configuration for ShodanClient. Bind it from the <c>Shodan</c> configuration section
///     (see <see cref="SectionName" />) or configure it in code via <c>AddShodanClient</c>.
/// </summary>
public sealed class ShodanClientOptions
{
	/// <summary>The default configuration section name (<c>Shodan</c>).</summary>
	public const string SectionName = "Shodan";

	/// <summary>
	///     The Shodan API key. Required for every surface except InternetDB. Prefer supplying it
	///     through configuration/secret storage (e.g. the environment variable
	///     <c>Shodan__ApiKey</c>) rather than hard-coding it.
	/// </summary>
	[Required(AllowEmptyStrings = false,
		ErrorMessage = "A Shodan API key is required. Set Shodan:ApiKey (or the Shodan__ApiKey environment variable).")]
	public required string ApiKey { get; set; } = string.Empty;

	/// <summary>Per-surface base-URL overrides. Leave unset to use Shodan's production hosts.</summary>
	public ShodanEndpointOptions Endpoints { get; set; } = new();

	/// <summary>
	///     Per-surface request timeouts. Not annotated <c>[ValidateObjectMembers]</c>: its properties
	///     carry no DataAnnotations (see <see cref="ShodanTimeoutOptions" /> for why) so there would be
	///     nothing to recurse into.
	/// </summary>
	public ShodanTimeoutOptions Timeouts { get; set; } = new();

	/// <summary>Client-side rate-limiting behavior.</summary>
	[ValidateObjectMembers]
	public ShodanRateLimitOptions RateLimit { get; set; } = new();

	/// <summary>Resilience (retry / circuit breaker / timeout) behavior for non-streaming surfaces.</summary>
	[ValidateObjectMembers]
	public ShodanResilienceOptions Resilience { get; set; } = new();
}
