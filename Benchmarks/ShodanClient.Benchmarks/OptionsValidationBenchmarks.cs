using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using ShodanClient.Application.Configuration;
using ShodanClient.Configuration;

namespace ShodanClient.Benchmarks;

/// <summary>
///     Compares the compile-time <c>[OptionsValidator]</c> source-generated validator
///     (<see cref="ValidateShodanClientOptions" />) against the reflection-based
///     <see cref="Validator" /> DataAnnotations path (<c>TryValidateObject</c>) for validating an
///     otherwise-valid <see cref="ShodanClientOptions" /> instance.
/// </summary>
[MemoryDiagnoser]
public class OptionsValidationBenchmarks
{
	private static readonly ShodanClientOptions Options = new() { ApiKey = "test-api-key" };

	[Benchmark(Baseline = true, Description = "Source-gen (ValidateShodanClientOptions)")]
	public bool SourceGenerated()
	{
		var validator = new ValidateShodanClientOptions();
		return validator.Validate(null, Options).Succeeded;
	}

	[Benchmark(Description = "Reflection (Validator.TryValidateObject)")]
	[UnconditionalSuppressMessage("AOT", "IL2026",
		Justification = "Deliberately benchmarking the reflection path against the source-gen path above.")]
	[UnconditionalSuppressMessage("AOT", "IL3050",
		Justification = "Deliberately benchmarking the reflection path against the source-gen path above.")]
	public bool Reflection()
	{
		var results = new List<ValidationResult>();
		return Validator.TryValidateObject(Options, new ValidationContext(Options), results, true);
	}
}
