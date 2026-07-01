using System.Text.Json.Serialization;

namespace ShodanClient.App.Services.Settings;

/// <summary>Source-generated serialization for <see cref="AppSettings" /> (AOT/trim safe).</summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal sealed partial class SettingsJsonContext : JsonSerializerContext;
