using System.Text.Json.Serialization;

namespace ShodanClient.App.Services.Credentials;

/// <summary>Source-generated serialization for <see cref="CredentialPayload" /> (AOT/trim safe).</summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CredentialPayload))]
[JsonSerializable(typeof(StoredProfile))]
internal sealed partial class CredentialJsonContext : JsonSerializerContext;
