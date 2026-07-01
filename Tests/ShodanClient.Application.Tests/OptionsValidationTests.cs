using System.ComponentModel.DataAnnotations;
using ShodanClient.Application.Configuration;

namespace ShodanClient.Application.Tests;

/// <summary>
///     Unit coverage for the DataAnnotations attributes ([Required], [Range]) declared on the
///     configuration option types. Exercises <see cref="Validator" />.TryValidateObject directly,
///     the same mechanism <c>Microsoft.Extensions.Options</c> uses under the hood for
///     <c>ValidateDataAnnotations()</c>. Properties that deliberately carry no DataAnnotations
///     attributes (the <see cref="TimeSpan" /> properties on <see cref="ShodanResilienceOptions" />
///     and every property on <see cref="ShodanTimeoutOptions" /> / <see cref="ShodanEndpointOptions" />,
///     kept AOT/trim-safe per their XML docs) are intentionally not covered here.
/// </summary>
public sealed class OptionsValidationTests
{
	private static bool TryValidate(object options, out List<ValidationResult> results)
	{
		results = [];
		return Validator.TryValidateObject(options, new ValidationContext(options), results, true);
	}

	public sealed class ShodanClientOptionsValidation
	{
		[Fact]
		public void ApiKey_with_non_empty_value_passes_validation()
		{
			var options = new ShodanClientOptions { ApiKey = "test-api-key" };

			var isValid = TryValidate(options, out var results);

			Assert.True(isValid);
			Assert.Empty(results);
		}

		[Fact]
		public void ApiKey_with_empty_string_fails_validation_and_reports_the_property()
		{
			var options = new ShodanClientOptions { ApiKey = string.Empty };

			var isValid = TryValidate(options, out var results);

			Assert.False(isValid);
			Assert.Contains(results, r => r.MemberNames.Contains(nameof(ShodanClientOptions.ApiKey)));
		}

		[Fact]
		public void ApiKey_with_whitespace_only_value_fails_validation()
		{
			var options = new ShodanClientOptions { ApiKey = "   " };

			var isValid = TryValidate(options, out var results);

			Assert.False(isValid);
			Assert.Contains(results, r => r.MemberNames.Contains(nameof(ShodanClientOptions.ApiKey)));
		}
	}

	public sealed class ShodanRateLimitOptionsValidation
	{
		[Fact]
		public void Defaults_pass_validation()
		{
			var options = new ShodanRateLimitOptions();

			var isValid = TryValidate(options, out var results);

			Assert.True(isValid);
			Assert.Empty(results);
		}

		[Fact]
		public void PermitsPerSecond_with_zero_fails_validation_and_reports_the_property()
		{
			var options = new ShodanRateLimitOptions { PermitsPerSecond = 0 };

			var isValid = TryValidate(options, out var results);

			Assert.False(isValid);
			Assert.Contains(results, r => r.MemberNames.Contains(nameof(ShodanRateLimitOptions.PermitsPerSecond)));
		}

		[Fact]
		public void PermitsPerSecond_with_negative_value_fails_validation()
		{
			var options = new ShodanRateLimitOptions { PermitsPerSecond = -1 };

			var isValid = TryValidate(options, out var results);

			Assert.False(isValid);
			Assert.Contains(results, r => r.MemberNames.Contains(nameof(ShodanRateLimitOptions.PermitsPerSecond)));
		}

		[Fact]
		public void PermitsPerSecond_with_one_passes_validation()
		{
			var options = new ShodanRateLimitOptions { PermitsPerSecond = 1 };

			var isValid = TryValidate(options, out var results);

			Assert.True(isValid);
			Assert.Empty(results);
		}

		[Fact]
		public void Burst_with_zero_fails_validation_and_reports_the_property()
		{
			var options = new ShodanRateLimitOptions { Burst = 0 };

			var isValid = TryValidate(options, out var results);

			Assert.False(isValid);
			Assert.Contains(results, r => r.MemberNames.Contains(nameof(ShodanRateLimitOptions.Burst)));
		}

		[Fact]
		public void Burst_with_one_passes_validation()
		{
			var options = new ShodanRateLimitOptions { Burst = 1 };

			var isValid = TryValidate(options, out var results);

			Assert.True(isValid);
			Assert.Empty(results);
		}

		[Fact]
		public void QueueLimit_with_negative_value_fails_validation_and_reports_the_property()
		{
			var options = new ShodanRateLimitOptions { QueueLimit = -1 };

			var isValid = TryValidate(options, out var results);

			Assert.False(isValid);
			Assert.Contains(results, r => r.MemberNames.Contains(nameof(ShodanRateLimitOptions.QueueLimit)));
		}

		[Fact]
		public void QueueLimit_with_zero_passes_validation()
		{
			var options = new ShodanRateLimitOptions { QueueLimit = 0 };

			var isValid = TryValidate(options, out var results);

			Assert.True(isValid);
			Assert.Empty(results);
		}
	}

	public sealed class ShodanResilienceOptionsValidation
	{
		[Fact]
		public void Defaults_pass_validation()
		{
			var options = new ShodanResilienceOptions();

			var isValid = TryValidate(options, out var results);

			Assert.True(isValid);
			Assert.Empty(results);
		}

		[Fact]
		public void MaxRetries_with_negative_value_fails_validation_and_reports_the_property()
		{
			var options = new ShodanResilienceOptions { MaxRetries = -1 };

			var isValid = TryValidate(options, out var results);

			Assert.False(isValid);
			Assert.Contains(results, r => r.MemberNames.Contains(nameof(ShodanResilienceOptions.MaxRetries)));
		}

		[Fact]
		public void MaxRetries_with_value_above_maximum_fails_validation_and_reports_the_property()
		{
			var options = new ShodanResilienceOptions { MaxRetries = 11 };

			var isValid = TryValidate(options, out var results);

			Assert.False(isValid);
			Assert.Contains(results, r => r.MemberNames.Contains(nameof(ShodanResilienceOptions.MaxRetries)));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(10)]
		public void MaxRetries_with_boundary_values_passes_validation(int maxRetries)
		{
			var options = new ShodanResilienceOptions { MaxRetries = maxRetries };

			var isValid = TryValidate(options, out var results);

			Assert.True(isValid);
			Assert.Empty(results);
		}
	}
}
