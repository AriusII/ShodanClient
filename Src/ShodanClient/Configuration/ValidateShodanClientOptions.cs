using Microsoft.Extensions.Options;
using ShodanClient.Application.Configuration;

namespace ShodanClient.Configuration;

/// <summary>
///     Source-generated validator for <see cref="ShodanClientOptions" />. The <c>[OptionsValidator]</c>
///     attribute generates a reflection-free (AOT-safe) implementation that enforces the DataAnnotations
///     on the options — most importantly that an API key is present.
/// </summary>
[OptionsValidator]
public sealed partial class ValidateShodanClientOptions : IValidateOptions<ShodanClientOptions>;
