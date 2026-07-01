using ShodanClient.Application.Common;
using ShodanClient.Application.Exceptions;

namespace ShodanClient.Application.Tests;

/// <summary>
///     Unit coverage for <see cref="Guard" /> — the client-side validation helpers used by the
///     service (logic) layer. Every failure must throw <see cref="ShodanRequestValidationException" />
///     with the offending parameter name captured via the caller-argument-expression attribute.
/// </summary>
public sealed class GuardTests
{
	[Fact]
	public void NotNullOrWhiteSpace_with_non_blank_string_returns_the_same_value()
	{
		var result = Guard.NotNullOrWhiteSpace("apache");

		Assert.Equal("apache", result);
	}

	[Fact]
	public void NotNullOrWhiteSpace_with_null_throws_and_captures_parameter_name()
	{
		string? query = null;

		var ex = Assert.Throws<ShodanRequestValidationException>(() => Guard.NotNullOrWhiteSpace(query));

		Assert.Equal("query", ex.ParameterName);
		Assert.Contains("'query'", ex.Message);
		Assert.Contains("must not be null or whitespace", ex.Message);
	}

	[Fact]
	public void NotNullOrWhiteSpace_with_empty_string_throws()
	{
		var ex = Assert.Throws<ShodanRequestValidationException>(() => Guard.NotNullOrWhiteSpace(string.Empty));

		Assert.Contains("must not be null or whitespace", ex.Message);
	}

	[Fact]
	public void NotNullOrWhiteSpace_with_whitespace_only_string_throws()
	{
		var ex = Assert.Throws<ShodanRequestValidationException>(() => Guard.NotNullOrWhiteSpace("   "));

		Assert.Contains("must not be null or whitespace", ex.Message);
	}

	[Fact]
	public void NotNullOrEmpty_list_with_populated_list_returns_the_same_instance()
	{
		IReadOnlyList<string> facets = ["country", "org"];

		var result = Guard.NotNullOrEmpty(facets);

		Assert.Same(facets, result);
	}

	[Fact]
	public void NotNullOrEmpty_list_with_null_throws_and_captures_parameter_name()
	{
		IReadOnlyList<string>? facets = null;

		var ex = Assert.Throws<ShodanRequestValidationException>(() => Guard.NotNullOrEmpty(facets));

		Assert.Equal("facets", ex.ParameterName);
		Assert.Contains("must contain at least one element", ex.Message);
	}

	[Fact]
	public void NotNullOrEmpty_list_with_empty_list_throws()
	{
		IReadOnlyList<string> facets = [];

		var ex = Assert.Throws<ShodanRequestValidationException>(() => Guard.NotNullOrEmpty(facets));

		Assert.Contains("must contain at least one element", ex.Message);
	}

	[Fact]
	public void NotNullOrEmpty_dictionary_with_populated_dictionary_returns_the_same_instance()
	{
		IReadOnlyDictionary<string, int> ports = new Dictionary<string, int> { ["ssh"] = 22 };

		var result = Guard.NotNullOrEmpty(ports);

		Assert.Same(ports, result);
	}

	[Fact]
	public void NotNullOrEmpty_dictionary_with_null_throws_and_captures_parameter_name()
	{
		IReadOnlyDictionary<string, int>? ports = null;

		var ex = Assert.Throws<ShodanRequestValidationException>(() => Guard.NotNullOrEmpty(ports));

		Assert.Equal("ports", ex.ParameterName);
		Assert.Contains("must contain at least one entry", ex.Message);
	}

	[Fact]
	public void NotNullOrEmpty_dictionary_with_empty_dictionary_throws()
	{
		IReadOnlyDictionary<string, int> ports = new Dictionary<string, int>();

		var ex = Assert.Throws<ShodanRequestValidationException>(() => Guard.NotNullOrEmpty(ports));

		Assert.Contains("must contain at least one entry", ex.Message);
	}

	[Fact]
	public void AtLeast_with_value_greater_than_min_returns_the_same_value()
	{
		var result = Guard.AtLeast(5, 1);

		Assert.Equal(5, result);
	}

	[Fact]
	public void AtLeast_with_value_exactly_at_min_returns_the_same_value()
	{
		var result = Guard.AtLeast(1, 1);

		Assert.Equal(1, result);
	}

	[Fact]
	public void AtLeast_with_value_below_min_throws_and_captures_parameter_name_and_bounds()
	{
		var page = 0;

		var ex = Assert.Throws<ShodanRequestValidationException>(() => Guard.AtLeast(page, 1));

		Assert.Equal("page", ex.ParameterName);
		Assert.Contains("'page'", ex.Message);
		Assert.Contains("greater than or equal to 1", ex.Message);
		Assert.Contains("was 0", ex.Message);
	}

	[Theory]
	[InlineData("8.8.8.8")]
	[InlineData("255.255.255.255")]
	[InlineData("::1")]
	[InlineData("2001:4860:4860::8888")]
	public void ValidIpAddress_with_valid_ip_returns_the_same_value(string ip)
	{
		var result = Guard.ValidIpAddress(ip);

		Assert.Equal(ip, result);
	}

	[Fact]
	public void ValidIpAddress_with_null_throws_with_the_not_null_or_whitespace_message()
	{
		string? ip = null;

		var ex = Assert.Throws<ShodanRequestValidationException>(() => Guard.ValidIpAddress(ip));

		Assert.Equal("ip", ex.ParameterName);
		Assert.Contains("must not be null or whitespace", ex.Message);
	}

	[Theory]
	[InlineData("not-an-ip")]
	[InlineData("999.999.999.999")]
	[InlineData("8.8.8.8.8")]
	public void ValidIpAddress_with_malformed_ip_throws_and_captures_parameter_name(string ip)
	{
		var ex = Assert.Throws<ShodanRequestValidationException>(() => Guard.ValidIpAddress(ip));

		Assert.Equal("ip", ex.ParameterName);
		Assert.Contains($"'{ip}'", ex.Message);
		Assert.Contains("must be a valid IP address", ex.Message);
	}
}
