using ShodanClient.Domain.Alerts;
using ShodanClient.Domain.Common;
using ShodanClient.Domain.Search;

namespace ShodanClient.Domain.Tests;

/// <summary>
///     Unit coverage for the Domain layer. Almost every type here is a plain sealed record with
///     required/init properties and collection-expression defaults — auto-generated record
///     equality/ToString is not our code and isn't worth testing. This file focuses on the two
///     value objects that carry genuine logic (<see cref="Cve" /> and <see cref="Cpe" />) plus a
///     handful of "shape sanity" checks locking in that collection-typed properties on the central
///     aggregate records default to an empty collection rather than null.
/// </summary>
public sealed class DomainModelTests
{
	// ---- Cve -------------------------------------------------------------------------------

	[Fact]
	public void Cve_constructor_normalizes_value_to_upper_invariant_and_trims_whitespace()
	{
		var cve = new Cve("  cve-2021-44228  ");

		Assert.Equal("CVE-2021-44228", cve.Value);
	}

	[Fact]
	public void Cve_constructor_with_null_throws_ArgumentNullException() =>
		Assert.Throws<ArgumentNullException>(() => new Cve(null!));

	[Fact]
	public void Cve_constructor_with_whitespace_only_throws_ArgumentException() =>
		Assert.Throws<ArgumentException>(() => new Cve("   "));

	[Fact]
	public void Cve_default_instance_has_empty_value_and_HasValue_false()
	{
		var cve = default(Cve);

		Assert.Equal(string.Empty, cve.Value);
		Assert.False(cve.HasValue);
	}

	[Fact]
	public void Cve_constructed_instance_has_HasValue_true()
	{
		var cve = new Cve("CVE-2021-44228");

		Assert.True(cve.HasValue);
	}

	[Fact]
	public void Cve_Parse_normalizes_like_the_constructor()
	{
		var cve = Cve.Parse("cve-2021-44228");

		Assert.Equal("CVE-2021-44228", cve.Value);
	}

	[Fact]
	public void Cve_TryParse_with_valid_value_returns_true_and_normalized_instance()
	{
		var result = Cve.TryParse("cve-2021-44228", out var cve);

		Assert.True(result);
		Assert.Equal("CVE-2021-44228", cve.Value);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Cve_TryParse_with_null_or_whitespace_returns_false_and_default_instance(string? value)
	{
		var result = Cve.TryParse(value, out var cve);

		Assert.False(result);
		Assert.Equal(default, cve);
	}

	[Fact]
	public void Cve_ToString_returns_the_normalized_value()
	{
		var cve = new Cve("cve-2021-44228");

		Assert.Equal("CVE-2021-44228", cve.ToString());
	}

	[Fact]
	public void Cve_equality_is_value_based_after_normalization()
	{
		var a = new Cve("cve-2021-44228");
		var b = new Cve("CVE-2021-44228");

		Assert.Equal(a, b);
	}

	// ---- Cpe -------------------------------------------------------------------------------

	[Fact]
	public void Cpe_constructor_trims_whitespace_but_does_not_change_case()
	{
		var cpe = new Cpe("  cpe:2.3:a:apache:http_server:2.4.1  ");

		Assert.Equal("cpe:2.3:a:apache:http_server:2.4.1", cpe.Value);
	}

	[Fact]
	public void Cpe_constructor_with_null_throws_ArgumentNullException() =>
		Assert.Throws<ArgumentNullException>(() => new Cpe(null!));

	[Fact]
	public void Cpe_constructor_with_whitespace_only_throws_ArgumentException() =>
		Assert.Throws<ArgumentException>(() => new Cpe("   "));

	[Fact]
	public void Cpe_default_instance_has_empty_value_and_HasValue_false()
	{
		var cpe = default(Cpe);

		Assert.Equal(string.Empty, cpe.Value);
		Assert.False(cpe.HasValue);
	}

	[Theory]
	[InlineData("cpe:2.3:a:apache:http_server:2.4.1", true)]
	[InlineData("CPE:2.3:a:apache:http_server:2.4.1", true)]
	[InlineData("cpe:/a:apache:http_server:2.4.1", false)]
	public void Cpe_IsVersion23_reflects_the_prefix_case_insensitively(string value, bool expected)
	{
		var cpe = new Cpe(value);

		Assert.Equal(expected, cpe.IsVersion23);
	}

	[Fact]
	public void Cpe_IsVersion23_on_default_instance_is_false()
	{
		var cpe = default(Cpe);

		Assert.False(cpe.IsVersion23);
	}

	[Fact]
	public void Cpe_Parse_produces_the_same_value_as_the_constructor()
	{
		var cpe = Cpe.Parse("cpe:2.3:a:apache:http_server:2.4.1");

		Assert.Equal("cpe:2.3:a:apache:http_server:2.4.1", cpe.Value);
	}

	[Fact]
	public void Cpe_TryParse_with_valid_value_returns_true()
	{
		var result = Cpe.TryParse("cpe:2.3:a:apache:http_server:2.4.1", out var cpe);

		Assert.True(result);
		Assert.Equal("cpe:2.3:a:apache:http_server:2.4.1", cpe.Value);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Cpe_TryParse_with_null_or_whitespace_returns_false_and_default_instance(string? value)
	{
		var result = Cpe.TryParse(value, out var cpe);

		Assert.False(result);
		Assert.Equal(default, cpe);
	}

	[Fact]
	public void Cpe_ToString_returns_the_value()
	{
		var cpe = new Cpe("cpe:2.3:a:apache:http_server:2.4.1");

		Assert.Equal("cpe:2.3:a:apache:http_server:2.4.1", cpe.ToString());
	}

	// ---- Shape sanity: collection defaults on central aggregate records --------------------

	[Fact]
	public void Host_with_only_required_properties_set_has_empty_collections_not_null()
	{
		var host = new Host { IpString = "8.8.8.8" };

		Assert.NotNull(host.Ports);
		Assert.Empty(host.Ports);
		Assert.NotNull(host.Hostnames);
		Assert.Empty(host.Hostnames);
		Assert.NotNull(host.Domains);
		Assert.Empty(host.Domains);
		Assert.NotNull(host.Tags);
		Assert.Empty(host.Tags);
		Assert.NotNull(host.Vulnerabilities);
		Assert.Empty(host.Vulnerabilities);
		Assert.NotNull(host.Services);
		Assert.Empty(host.Services);
	}

	[Fact]
	public void Banner_with_only_required_properties_set_has_empty_collections_not_null()
	{
		var banner = new Banner { IpString = "8.8.8.8" };

		Assert.NotNull(banner.Cpe);
		Assert.Empty(banner.Cpe);
		Assert.NotNull(banner.Cpe23);
		Assert.Empty(banner.Cpe23);
		Assert.NotNull(banner.Hostnames);
		Assert.Empty(banner.Hostnames);
		Assert.NotNull(banner.Domains);
		Assert.Empty(banner.Domains);
		Assert.NotNull(banner.Tags);
		Assert.Empty(banner.Tags);
		Assert.NotNull(banner.Vulnerabilities);
		Assert.Empty(banner.Vulnerabilities);
	}

	[Fact]
	public void Alert_with_only_required_properties_set_has_empty_collections_not_null()
	{
		var alert = new Alert { Id = "abc123", Name = "my-alert" };

		Assert.NotNull(alert.Filters);
		Assert.NotNull(alert.Filters.Ip);
		Assert.Empty(alert.Filters.Ip);
		Assert.NotNull(alert.Triggers);
		Assert.Empty(alert.Triggers);
		Assert.NotNull(alert.Notifiers);
		Assert.Empty(alert.Notifiers);
	}
}
