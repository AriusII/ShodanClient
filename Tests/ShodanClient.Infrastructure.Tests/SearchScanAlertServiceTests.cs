using NSubstitute;
using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Alerts;
using ShodanClient.Application.Exceptions;
using ShodanClient.Application.Scanning;
using ShodanClient.Application.Search;
using ShodanClient.Domain.Alerts;
using ShodanClient.Domain.Scanning;
using ShodanClient.Domain.Search;
using ShodanClient.Infrastructure.Alerts;
using ShodanClient.Infrastructure.Scanning;
using ShodanClient.Infrastructure.Search;

namespace ShodanClient.Infrastructure.Tests;

/// <summary>
///     Service-orchestration unit tests for <see cref="HostService" />, <see cref="SearchService" />,
///     <see cref="ScanService" /> and <see cref="AlertService" />: each repository is mocked with
///     NSubstitute so these tests exercise only the Guard-clause validation and delegation logic that
///     lives in the Infrastructure service layer, not any real HTTP/JSON behavior.
/// </summary>
public sealed class HostServiceTests
{
	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("not-an-ip")]
	[InlineData("999.999.999.999")]
	public async Task GetAsync_invalid_ip_throws_and_does_not_call_repository(string? ip)
	{
		var repo = Substitute.For<IHostRepository>();
		var service = new HostService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.GetAsync(ip!, cancellationToken: TestContext.Current.CancellationToken));

		await repo.DidNotReceive().GetAsync(Arg.Any<HostLookupQuery>(), Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData("8.8.8.8")]
	[InlineData("2001:4860:4860::8888")]
	public async Task GetAsync_valid_ip_delegates_to_repository_and_returns_result(string ip)
	{
		var repo = Substitute.For<IHostRepository>();
		var expected = new Host { IpString = ip };
		repo.GetAsync(Arg.Any<HostLookupQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new HostService(repo);

		var result = await service.GetAsync(ip, true, true, TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).GetAsync(
			Arg.Is<HostLookupQuery>(q => q.Ip == ip && q.History && q.Minify),
			Arg.Any<CancellationToken>());
	}
}

public sealed class SearchServiceTests
{
	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task SearchAsync_invalid_query_throws_and_does_not_call_repository(string? query)
	{
		var repo = Substitute.For<ISearchRepository>();
		var service = new SearchService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.SearchAsync(query!, cancellationToken: TestContext.Current.CancellationToken));

		await repo.DidNotReceive().SearchAsync(Arg.Any<HostSearchQuery>(), Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task SearchAsync_page_less_than_one_throws_and_does_not_call_repository(int page)
	{
		var repo = Substitute.For<ISearchRepository>();
		var service = new SearchService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.SearchAsync("port:80", page: page, cancellationToken: TestContext.Current.CancellationToken));

		await repo.DidNotReceive().SearchAsync(Arg.Any<HostSearchQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SearchAsync_valid_call_delegates_to_repository_and_returns_result()
	{
		var repo = Substitute.For<ISearchRepository>();
		var expected = new SearchResult { Total = 42 };
		repo.SearchAsync(Arg.Any<HostSearchQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new SearchService(repo);

		var result = await service.SearchAsync("port:80", "country", 2, TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).SearchAsync(
			Arg.Is<HostSearchQuery>(q => q.Query == "port:80" && q.Facets == "country" && q.Page == 2),
			Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task CountAsync_invalid_query_throws_and_does_not_call_repository(string? query)
	{
		var repo = Substitute.For<ISearchRepository>();
		var service = new SearchService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.CountAsync(query!, cancellationToken: TestContext.Current.CancellationToken));

		await repo.DidNotReceive().CountAsync(Arg.Any<HostCountQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CountAsync_valid_call_delegates_to_repository_and_returns_result()
	{
		var repo = Substitute.For<ISearchRepository>();
		var expected = new CountResult { Total = 7 };
		repo.CountAsync(Arg.Any<HostCountQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new SearchService(repo);

		var result = await service.CountAsync("port:443", "org", TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).CountAsync(
			Arg.Is<HostCountQuery>(q => q.Query == "port:443" && q.Facets == "org"),
			Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task ParseQueryAsync_invalid_query_throws_and_does_not_call_repository(string? query)
	{
		var repo = Substitute.For<ISearchRepository>();
		var service = new SearchService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.ParseQueryAsync(query!, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().ParseQueryAsync(Arg.Any<QueryTokenRequest>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ParseQueryAsync_valid_call_delegates_to_repository_and_returns_result()
	{
		var repo = Substitute.For<ISearchRepository>();
		var expected = new QueryTokenParse { SearchTerm = "apache" };
		repo.ParseQueryAsync(Arg.Any<QueryTokenRequest>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new SearchService(repo);

		var result = await service.ParseQueryAsync("apache port:80", TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).ParseQueryAsync(
			Arg.Is<QueryTokenRequest>(q => q.Query == "apache port:80"),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SearchAllAsync_invalid_query_throws_and_does_not_call_repository()
	{
		var repo = Substitute.For<ISearchRepository>();
		var service = new SearchService(repo);

		async Task Act()
		{
			await foreach (var _ in service.SearchAllAsync(" ",
							   cancellationToken: TestContext.Current.CancellationToken))
			{
			}
		}

		await Assert.ThrowsAsync<ShodanRequestValidationException>(Act);
		await repo.DidNotReceive().SearchAsync(Arg.Any<HostSearchQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SearchAllAsync_startPage_less_than_one_throws_and_does_not_call_repository()
	{
		var repo = Substitute.For<ISearchRepository>();
		var service = new SearchService(repo);

		async Task Act()
		{
			await foreach (var _ in service.SearchAllAsync(
							   "port:80", 0, cancellationToken: TestContext.Current.CancellationToken))
			{
			}
		}

		await Assert.ThrowsAsync<ShodanRequestValidationException>(Act);
		await repo.DidNotReceive().SearchAsync(Arg.Any<HostSearchQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SearchAllAsync_pages_through_multiple_pages_until_total_is_reached()
	{
		var repo = Substitute.For<ISearchRepository>();
		var page1 = CreateBanners(100, "p1-");
		var page2 = CreateBanners(50, "p2-");
		repo.SearchAsync(Arg.Is<HostSearchQuery>(q => q.Page == 1), Arg.Any<CancellationToken>())
			.Returns(new SearchResult { Matches = page1, Total = 150 });
		repo.SearchAsync(Arg.Is<HostSearchQuery>(q => q.Page == 2), Arg.Any<CancellationToken>())
			.Returns(new SearchResult { Matches = page2, Total = 150 });
		var service = new SearchService(repo);

		var results = new List<Banner>();
		await foreach (var banner in service.SearchAllAsync(
						   "port:80", cancellationToken: TestContext.Current.CancellationToken))
		{
			results.Add(banner);
		}

		Assert.Equal(page1.Concat(page2), results);
		await repo.Received(1).SearchAsync(
			Arg.Is<HostSearchQuery>(q => q.Page == 1 && q.Query == "port:80" && q.Facets == null),
			Arg.Any<CancellationToken>());
		await repo.Received(1).SearchAsync(
			Arg.Is<HostSearchQuery>(q => q.Page == 2 && q.Query == "port:80" && q.Facets == null),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SearchAllAsync_stops_after_maxPages_even_if_more_results_remain()
	{
		var repo = Substitute.For<ISearchRepository>();
		var page = CreateBanners(100, "p-");
		repo.SearchAsync(Arg.Any<HostSearchQuery>(), Arg.Any<CancellationToken>())
			.Returns(new SearchResult { Matches = page, Total = 1000 });
		var service = new SearchService(repo);

		var results = new List<Banner>();
		await foreach (var banner in service.SearchAllAsync(
						   "port:80", maxPages: 1, cancellationToken: TestContext.Current.CancellationToken))
		{
			results.Add(banner);
		}

		Assert.Equal(100, results.Count);
		await repo.Received(1).SearchAsync(Arg.Any<HostSearchQuery>(), Arg.Any<CancellationToken>());
	}

	private static List<Banner> CreateBanners(int count, string prefix)
	{
		var banners = new List<Banner>(count);
		for (var i = 0; i < count; i++)
		{
			banners.Add(new Banner { IpString = $"{prefix}{i}" });
		}

		return banners;
	}
}

public sealed class ScanServiceTests
{
	[Fact]
	public async Task RequestScanAsync_null_targets_throws_and_does_not_call_repository()
	{
		var repo = Substitute.For<IScanRepository>();
		var service = new ScanService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.RequestScanAsync(null!, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().RequestScanAsync(Arg.Any<ScanRequestQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RequestScanAsync_empty_targets_throws_and_does_not_call_repository()
	{
		var repo = Substitute.For<IScanRepository>();
		var service = new ScanService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.RequestScanAsync([], TestContext.Current.CancellationToken));

		await repo.DidNotReceive().RequestScanAsync(Arg.Any<ScanRequestQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RequestScanAsync_valid_targets_joins_as_csv_and_delegates_to_repository()
	{
		var repo = Substitute.For<IScanRepository>();
		var expected = new ScanSubmission { Id = "scan-1", Count = 2, CreditsLeft = 98 };
		repo.RequestScanAsync(Arg.Any<ScanRequestQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new ScanService(repo);

		var result = await service.RequestScanAsync(["1.1.1.1", "2.2.2.2"], TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).RequestScanAsync(
			Arg.Is<ScanRequestQuery>(q => q.Ips == "1.1.1.1,2.2.2.2"),
			Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task ScanInternetAsync_invalid_port_throws_and_does_not_call_repository(int port)
	{
		var repo = Substitute.For<IScanRepository>();
		var service = new ScanService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.ScanInternetAsync(port, "https", TestContext.Current.CancellationToken));

		await repo.DidNotReceive().ScanInternetAsync(Arg.Any<ScanInternetQuery>(), Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task ScanInternetAsync_invalid_protocol_throws_and_does_not_call_repository(string? protocol)
	{
		var repo = Substitute.For<IScanRepository>();
		var service = new ScanService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.ScanInternetAsync(443, protocol!, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().ScanInternetAsync(Arg.Any<ScanInternetQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ScanInternetAsync_valid_call_delegates_to_repository_and_returns_result()
	{
		var repo = Substitute.For<IScanRepository>();
		var expected = new ScanInternetResult { Id = "internet-scan-1" };
		repo.ScanInternetAsync(Arg.Any<ScanInternetQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new ScanService(repo);

		var result = await service.ScanInternetAsync(443, "https", TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).ScanInternetAsync(
			Arg.Is<ScanInternetQuery>(q => q.Port == 443 && q.Protocol == "https"),
			Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task GetScanStatusAsync_invalid_scanId_throws_and_does_not_call_repository(string? scanId)
	{
		var repo = Substitute.For<IScanRepository>();
		var service = new ScanService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.GetScanStatusAsync(scanId!, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().GetScanStatusAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetScanStatusAsync_valid_call_delegates_to_repository_and_returns_result()
	{
		var repo = Substitute.For<IScanRepository>();
		var expected = new ScanStatus { Id = "scan-42", Status = "PROCESSING" };
		repo.GetScanStatusAsync("scan-42", Arg.Any<CancellationToken>()).Returns(expected);
		var service = new ScanService(repo);

		var result = await service.GetScanStatusAsync("scan-42", TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).GetScanStatusAsync("scan-42", Arg.Any<CancellationToken>());
	}
}

public sealed class AlertServiceTests
{
	// ----- CreateAsync ------------------------------------------------------

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task CreateAsync_invalid_name_throws_and_does_not_call_repository(string? name)
	{
		var repo = Substitute.For<IAlertRepository>();
		var service = new AlertService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.CreateAsync(name!, ["1.1.1.1"], cancellationToken: TestContext.Current.CancellationToken));

		await repo.DidNotReceive().CreateAsync(Arg.Any<CreateAlertQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateAsync_empty_ips_throws_and_does_not_call_repository()
	{
		var repo = Substitute.For<IAlertRepository>();
		var service = new AlertService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.CreateAsync("alert", [], cancellationToken: TestContext.Current.CancellationToken));

		await repo.DidNotReceive().CreateAsync(Arg.Any<CreateAlertQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateAsync_negative_expiresSeconds_throws_and_does_not_call_repository()
	{
		var repo = Substitute.For<IAlertRepository>();
		var service = new AlertService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.CreateAsync("alert", ["1.1.1.1"], -1, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().CreateAsync(Arg.Any<CreateAlertQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateAsync_valid_call_delegates_to_repository_and_returns_result()
	{
		var repo = Substitute.For<IAlertRepository>();
		var expected = new Alert { Id = "alert-1", Name = "my-alert" };
		repo.CreateAsync(Arg.Any<CreateAlertQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new AlertService(repo);

		var result = await service.CreateAsync(
			"my-alert", ["1.1.1.1", "2.2.2.0/24"], 3600, TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).CreateAsync(
			Arg.Is<CreateAlertQuery>(q =>
				q.Name == "my-alert" && q.Expires == 3600 && q.Ips.Count == 2 &&
				q.Ips[0] == "1.1.1.1" && q.Ips[1] == "2.2.2.0/24"),
			Arg.Any<CancellationToken>());
	}

	// ----- GetAsync -----------------------------------------------------------

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task GetAsync_invalid_id_throws_and_does_not_call_repository(string? id)
	{
		var repo = Substitute.For<IAlertRepository>();
		var service = new AlertService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.GetAsync(id!, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetAsync_valid_id_delegates_to_repository_and_returns_result()
	{
		var repo = Substitute.For<IAlertRepository>();
		var expected = new Alert { Id = "alert-1", Name = "my-alert" };
		repo.GetAsync("alert-1", Arg.Any<CancellationToken>()).Returns(expected);
		var service = new AlertService(repo);

		var result = await service.GetAsync("alert-1", TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).GetAsync("alert-1", Arg.Any<CancellationToken>());
	}

	// ----- UpdateNetworksAsync --------------------------------------------------

	[Fact]
	public async Task UpdateNetworksAsync_invalid_id_throws_and_does_not_call_repository()
	{
		var repo = Substitute.For<IAlertRepository>();
		var service = new AlertService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.UpdateNetworksAsync(" ", ["1.1.1.1"], TestContext.Current.CancellationToken));

		await repo.DidNotReceive()
			.UpdateNetworksAsync(Arg.Any<UpdateAlertNetworksQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UpdateNetworksAsync_empty_ips_throws_and_does_not_call_repository()
	{
		var repo = Substitute.For<IAlertRepository>();
		var service = new AlertService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.UpdateNetworksAsync("alert-1", [], TestContext.Current.CancellationToken));

		await repo.DidNotReceive()
			.UpdateNetworksAsync(Arg.Any<UpdateAlertNetworksQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UpdateNetworksAsync_valid_call_delegates_to_repository_and_returns_result()
	{
		var repo = Substitute.For<IAlertRepository>();
		var expected = new Alert { Id = "alert-1", Name = "my-alert" };
		repo.UpdateNetworksAsync(Arg.Any<UpdateAlertNetworksQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new AlertService(repo);

		var result = await service.UpdateNetworksAsync(
			"alert-1", ["3.3.3.3"], TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).UpdateNetworksAsync(
			Arg.Is<UpdateAlertNetworksQuery>(q => q.Id == "alert-1" && q.Ips.Count == 1 && q.Ips[0] == "3.3.3.3"),
			Arg.Any<CancellationToken>());
	}

	// ----- DeleteAsync ----------------------------------------------------------

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task DeleteAsync_invalid_id_throws_and_does_not_call_repository(string? id)
	{
		var repo = Substitute.For<IAlertRepository>();
		var service = new AlertService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.DeleteAsync(id!, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAsync_valid_id_delegates_to_repository_and_returns_result()
	{
		var repo = Substitute.For<IAlertRepository>();
		repo.DeleteAsync("alert-1", Arg.Any<CancellationToken>()).Returns(true);
		var service = new AlertService(repo);

		var result = await service.DeleteAsync("alert-1", TestContext.Current.CancellationToken);

		Assert.True(result);
		await repo.Received(1).DeleteAsync("alert-1", Arg.Any<CancellationToken>());
	}

	// ----- EnableTriggerAsync (two-string guard chain) ---------------------------

	[Theory]
	[InlineData(null, "malware")]
	[InlineData("alert-1", null)]
	[InlineData(" ", "malware")]
	[InlineData("alert-1", " ")]
	public async Task EnableTriggerAsync_invalid_id_or_trigger_throws_and_does_not_call_repository(
		string? id, string? trigger)
	{
		var repo = Substitute.For<IAlertRepository>();
		var service = new AlertService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.EnableTriggerAsync(id!, trigger!, TestContext.Current.CancellationToken));

		await repo.DidNotReceive()
			.EnableTriggerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EnableTriggerAsync_valid_call_delegates_raw_arguments_to_repository_and_returns_result()
	{
		var repo = Substitute.For<IAlertRepository>();
		repo.EnableTriggerAsync("alert-1", "malware", Arg.Any<CancellationToken>()).Returns(true);
		var service = new AlertService(repo);

		var result = await service.EnableTriggerAsync("alert-1", "malware", TestContext.Current.CancellationToken);

		Assert.True(result);
		await repo.Received(1).EnableTriggerAsync("alert-1", "malware", Arg.Any<CancellationToken>());
	}

	// ----- IgnoreTriggerServiceAsync (three-string guard chain) ------------------

	[Theory]
	[InlineData(null, "malware", "1.1.1.1:80")]
	[InlineData("alert-1", null, "1.1.1.1:80")]
	[InlineData("alert-1", "malware", null)]
	[InlineData("alert-1", "malware", "   ")]
	public async Task IgnoreTriggerServiceAsync_invalid_argument_throws_and_does_not_call_repository(
		string? id, string? trigger, string? service)
	{
		var repo = Substitute.For<IAlertRepository>();
		var alertService = new AlertService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			alertService.IgnoreTriggerServiceAsync(id!, trigger!, service!, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().IgnoreTriggerServiceAsync(
			Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task IgnoreTriggerServiceAsync_valid_call_delegates_raw_arguments_to_repository_and_returns_result()
	{
		var repo = Substitute.For<IAlertRepository>();
		repo.IgnoreTriggerServiceAsync("alert-1", "malware", "1.1.1.1:80", Arg.Any<CancellationToken>())
			.Returns(true);
		var service = new AlertService(repo);

		var result = await service.IgnoreTriggerServiceAsync(
			"alert-1", "malware", "1.1.1.1:80", TestContext.Current.CancellationToken);

		Assert.True(result);
		await repo.Received(1).IgnoreTriggerServiceAsync(
			"alert-1", "malware", "1.1.1.1:80", Arg.Any<CancellationToken>());
	}
}
