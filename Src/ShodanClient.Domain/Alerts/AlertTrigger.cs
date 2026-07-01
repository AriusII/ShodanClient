namespace ShodanClient.Domain.Alerts;

/// <summary>Configuration of a single trigger enabled on an alert (an <see cref="Alert.Triggers" /> value).</summary>
public sealed record AlertTrigger
{
	/// <summary>The trigger's rule expression, if customized (<c>rule</c>).</summary>
	public string? Rule { get; init; }
}
