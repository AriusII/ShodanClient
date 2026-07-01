using ShodanClient.Infrastructure.Http.Routing;
using ShodanClient.Infrastructure.Serialization;

namespace ShodanClient.Infrastructure.Tests;

/// <summary>
///     Unit coverage for <see cref="QueryStringBuilder" /> (the allocation-conscious route composer)
///     and <see cref="ShodanValueParsers" /> (the tz-less Shodan timestamp parser).
/// </summary>
public sealed class RoutingAndValueParsersTests
{
	[Fact]
	public void Build_with_no_appends_returns_empty_string()
	{
		var builder = new QueryStringBuilder();

		var result = builder.Build();

		Assert.Equal(string.Empty, result);
	}

	[Fact]
	public void Add_string_then_build_produces_leading_question_mark()
	{
		var builder = new QueryStringBuilder();
		builder.Add("query", "apache");

		var result = builder.Build();

		Assert.Equal("?query=apache", result);
	}

	[Fact]
	public void Add_multiple_params_separates_them_with_ampersand()
	{
		var builder = new QueryStringBuilder();
		builder.Add("query", "apache");
		builder.Add("page", 2);
		builder.Add("facets", "country");

		var result = builder.Build();

		Assert.Equal("?query=apache&page=2&facets=country", result);
	}

	[Fact]
	public void Add_int_uses_invariant_culture_formatting()
	{
		var builder = new QueryStringBuilder();
		builder.Add("page", 12345);

		var result = builder.Build();

		Assert.Equal("?page=12345", result);
	}

	[Fact]
	public void Add_url_escapes_special_characters_in_the_value()
	{
		const string rawValue = "port:22 country:\"US\"&more";
		var builder = new QueryStringBuilder();
		builder.Add("query", rawValue);

		var result = builder.Build();

		Assert.Equal("?query=" + Uri.EscapeDataString(rawValue), result);
		// The value's own "&" must be escaped away, so the only literal "&" left is one we didn't add.
		Assert.DoesNotContain("&more", result);
		Assert.DoesNotContain(" ", result);
	}

	[Fact]
	public void Build_called_a_second_time_no_longer_returns_the_composed_query_string()
	{
		// QueryStringBuilder is documented as "call [Build] exactly once": it hands its pooled buffer
		// back via DefaultInterpolatedStringHandler.ToStringAndClear(), which resets the underlying
		// handler. A second Build() call therefore reads from an already-cleared handler.
		var builder = new QueryStringBuilder();
		builder.Add("query", "apache");

		var first = builder.Build();
		var second = builder.Build();

		Assert.Equal("?query=apache", first);
		Assert.NotEqual(first, second);
	}

	[Fact]
	public void AddFlag_appends_name_equals_true_only_when_value_is_true()
	{
		var builder = new QueryStringBuilder();
		builder.AddFlag("minify", true);
		builder.AddFlag("history", false);

		var result = builder.Build();

		Assert.Equal("?minify=true", result);
	}

	[Fact]
	public void AddFlag_with_false_value_appends_nothing()
	{
		var builder = new QueryStringBuilder();
		builder.AddFlag("history", false);

		var result = builder.Build();

		Assert.Equal(string.Empty, result);
	}

	[Fact]
	public void AddIfPresent_with_null_appends_nothing()
	{
		var builder = new QueryStringBuilder();
		builder.AddIfPresent("facets", null);

		var result = builder.Build();

		Assert.Equal(string.Empty, result);
	}

	[Fact]
	public void AddIfPresent_with_empty_string_appends_nothing()
	{
		var builder = new QueryStringBuilder();
		builder.AddIfPresent("facets", string.Empty);

		var result = builder.Build();

		Assert.Equal(string.Empty, result);
	}

	[Fact]
	public void AddIfPresent_with_non_empty_value_appends_the_parameter()
	{
		var builder = new QueryStringBuilder();
		builder.AddIfPresent("facets", "country");

		var result = builder.Build();

		Assert.Equal("?facets=country", result);
	}

	[Fact]
	public void ParseTimestamp_parses_iso_timestamp_with_microseconds_as_utc()
	{
		var result = ShodanValueParsers.ParseTimestamp("2020-09-29T09:39:45.813661");

		var expected = new DateTimeOffset(new DateTime(2020, 9, 29, 9, 39, 45, DateTimeKind.Utc).AddTicks(8136610));
		Assert.NotNull(result);
		Assert.Equal(TimeSpan.Zero, result.Value.Offset);
		Assert.Equal(expected, result.Value);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void ParseTimestamp_returns_null_for_null_or_blank_input(string? value)
	{
		var result = ShodanValueParsers.ParseTimestamp(value);

		Assert.Null(result);
	}

	[Fact]
	public void ParseTimestamp_returns_null_for_malformed_input_instead_of_throwing()
	{
		var result = ShodanValueParsers.ParseTimestamp("not-a-timestamp");

		Assert.Null(result);
	}
}
