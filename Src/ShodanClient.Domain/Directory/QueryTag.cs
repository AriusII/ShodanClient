namespace ShodanClient.Domain.Directory;

/// <summary>
///     A single popular-tag bucket from the saved-query directory (e.g. <c>value = "webcam", count = 209</c>).
/// </summary>
/// <param name="Value">The tag text.</param>
/// <param name="Count">The number of saved queries carrying the tag.</param>
public readonly record struct QueryTag(string Value, long Count);
