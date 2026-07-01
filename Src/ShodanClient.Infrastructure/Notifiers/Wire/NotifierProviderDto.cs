namespace ShodanClient.Infrastructure.Notifiers.Wire;

/// <summary>
///     Wire shape of a single provider entry in the <c>GET /notifier/provider</c> response (the value
///     of the <c>{ &lt;provider&gt;: { required: [...] } }</c> map).
/// </summary>
internal sealed class NotifierProviderDto
{
	public string[]? Required { get; set; }
}
