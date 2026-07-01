using NSubstitute;
using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Dns;
using ShodanClient.Application.Exceptions;
using ShodanClient.Domain.ApiStatus;
using ShodanClient.Domain.Dns;
using ShodanClient.Domain.InternetDb;
using ShodanClient.Infrastructure.ApiStatus;
using ShodanClient.Infrastructure.Dns;
using ShodanClient.Infrastructure.InternetDb;
using ShodanClient.Infrastructure.Utility;

namespace ShodanClient.Infrastructure.Tests;

/// <summary>
///     Service-orchestration unit tests for <see cref="DnsService" />: the repository is mocked with
///     NSubstitute so these tests exercise only the Guard-clause validation, CSV-joining and
///     delegation logic that lives in the Infrastructure service layer, not any real HTTP/JSON
///     behavior.
/// </summary>
public sealed class DnsServiceTests
{
	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task GetDomainAsync_blank_domain_throws_validation_exception_without_calling_repository(
		string? domain)
	{
		var repo = Substitute.For<IDnsRepository>();
		var service = new DnsService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.GetDomainAsync(domain!, cancellationToken: TestContext.Current.CancellationToken));

		await repo.DidNotReceive().GetDomainAsync(Arg.Any<DomainDnsQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetDomainAsync_valid_input_builds_query_and_returns_repository_result()
	{
		var repo = Substitute.For<IDnsRepository>();
		var expected = new DomainDnsInfo { Domain = "example.com" };
		repo.GetDomainAsync(Arg.Any<DomainDnsQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new DnsService(repo);

		var result = await service.GetDomainAsync(
			"example.com", true, "A", 2, TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).GetDomainAsync(
			Arg.Is<DomainDnsQuery>(q => q.Domain == "example.com" && q.History && q.Type == "A" && q.Page == 2),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ResolveAsync_empty_hostnames_throws_validation_exception_without_calling_repository()
	{
		var repo = Substitute.For<IDnsRepository>();
		var service = new DnsService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.ResolveAsync([], TestContext.Current.CancellationToken));

		await repo.DidNotReceive().ResolveAsync(Arg.Any<DnsResolveQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ResolveAsync_valid_input_joins_hostnames_into_csv_and_returns_repository_result()
	{
		var repo = Substitute.For<IDnsRepository>();
		IReadOnlyDictionary<string, string?> expected =
			new Dictionary<string, string?> { ["example.com"] = "93.184.216.34", ["missing.example.com"] = null };
		repo.ResolveAsync(Arg.Any<DnsResolveQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new DnsService(repo);

		var result = await service.ResolveAsync(
			["example.com", "missing.example.com"], TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).ResolveAsync(
			Arg.Is<DnsResolveQuery>(q => q.HostnamesCsv == "example.com,missing.example.com"),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ReverseAsync_empty_ips_throws_validation_exception_without_calling_repository()
	{
		var repo = Substitute.For<IDnsRepository>();
		var service = new DnsService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.ReverseAsync([], TestContext.Current.CancellationToken));

		await repo.DidNotReceive().ReverseAsync(Arg.Any<DnsReverseQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ReverseAsync_valid_input_joins_ips_into_csv_and_returns_repository_result()
	{
		var repo = Substitute.For<IDnsRepository>();
		IReadOnlyDictionary<string, IReadOnlyList<string>> expected =
			new Dictionary<string, IReadOnlyList<string>> { ["8.8.8.8"] = ["dns.google"] };
		repo.ReverseAsync(Arg.Any<DnsReverseQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new DnsService(repo);

		var result = await service.ReverseAsync(["8.8.8.8", "1.1.1.1"], TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).ReverseAsync(
			Arg.Is<DnsReverseQuery>(q => q.IpsCsv == "8.8.8.8,1.1.1.1"), Arg.Any<CancellationToken>());
	}
}

/// <summary>
///     Service-orchestration unit tests for <see cref="UtilityService" />: the repository is mocked
///     with NSubstitute so these tests verify only the service's own responsibility - delegation -
///     without any HTTP involved. <see cref="UtilityService" /> has no guard clauses to validate.
/// </summary>
public sealed class UtilityServiceTests
{
	[Fact]
	public async Task GetHttpHeadersAsync_delegates_to_repository_and_returns_result_unchanged()
	{
		var repo = Substitute.For<IUtilityRepository>();
		IReadOnlyDictionary<string, string> expected = new Dictionary<string, string> { ["User-Agent"] = "curl/8.0" };
		repo.GetHttpHeadersAsync(Arg.Any<CancellationToken>()).Returns(expected);
		var service = new UtilityService(repo);

		var result = await service.GetHttpHeadersAsync(TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).GetHttpHeadersAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetMyIpAsync_delegates_to_repository_and_returns_result_unchanged()
	{
		var repo = Substitute.For<IUtilityRepository>();
		repo.GetMyIpAsync(Arg.Any<CancellationToken>()).Returns("203.0.113.42");
		var service = new UtilityService(repo);

		var result = await service.GetMyIpAsync(TestContext.Current.CancellationToken);

		Assert.Equal("203.0.113.42", result);
		await repo.Received(1).GetMyIpAsync(Arg.Any<CancellationToken>());
	}
}

/// <summary>
///     Service-orchestration unit tests for <see cref="ApiStatusService" />: the repository is mocked
///     with NSubstitute so these tests verify only the service's own responsibility - delegation -
///     without any HTTP involved. <see cref="ApiStatusService" /> has no guard clauses to validate.
/// </summary>
public sealed class ApiStatusServiceTests
{
	[Fact]
	public async Task GetAsync_delegates_to_repository_and_returns_result_unchanged()
	{
		var repo = Substitute.For<IApiStatusRepository>();
		var expected = new ApiInfo
		{
			ScanCredits = 10,
			QueryCredits = 20,
			Plan = "dev",
			Https = true,
			Telnet = false,
			Unlocked = true,
			UnlockedLeft = 3,
			UsageLimits = new UsageLimits { ScanCredits = 100, QueryCredits = 200 }
		};
		repo.GetAsync(Arg.Any<CancellationToken>()).Returns(expected);
		var service = new ApiStatusService(repo);

		var result = await service.GetAsync(TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).GetAsync(Arg.Any<CancellationToken>());
	}
}

/// <summary>
///     Service-orchestration unit tests for <see cref="InternetDbService" />: the repository is mocked
///     with NSubstitute so these tests exercise only the Guard-clause IP validation and delegation
///     logic that lives in the Infrastructure service layer, not any real HTTP/JSON behavior.
/// </summary>
public sealed class InternetDbServiceTests
{
	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("not-an-ip")]
	[InlineData("999.999.999.999")]
	public async Task GetAsync_invalid_ip_throws_validation_exception_without_calling_repository(string? ip)
	{
		var repo = Substitute.For<IInternetDbRepository>();
		var service = new InternetDbService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.GetAsync(ip!, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData("8.8.8.8")]
	[InlineData("2001:4860:4860::8888")]
	public async Task GetAsync_valid_ip_delegates_to_repository_and_returns_result_unchanged(string ip)
	{
		var repo = Substitute.For<IInternetDbRepository>();
		var expected = new InternetDbHost { Ip = ip, Ports = [53, 443] };
		repo.GetAsync(ip, Arg.Any<CancellationToken>()).Returns(expected);
		var service = new InternetDbService(repo);

		var result = await service.GetAsync(ip, TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).GetAsync(ip, Arg.Any<CancellationToken>());
	}
}
