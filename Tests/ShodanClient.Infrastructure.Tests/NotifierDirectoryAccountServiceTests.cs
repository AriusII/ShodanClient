using NSubstitute;
using ShodanClient.Application.Abstractions.Repositories;
using ShodanClient.Application.Directory;
using ShodanClient.Application.Exceptions;
using ShodanClient.Application.Notifiers;
using ShodanClient.Domain.Account;
using ShodanClient.Domain.Directory;
using ShodanClient.Domain.Notifiers;
using ShodanClient.Infrastructure.Account;
using ShodanClient.Infrastructure.Directory;
using ShodanClient.Infrastructure.Notifiers;

namespace ShodanClient.Infrastructure.Tests;

/// <summary>
///     Service-orchestration unit tests for <see cref="NotifierService" />: the repository is mocked
///     with NSubstitute so these tests verify only the service's own responsibilities - guard-clause
///     validation and delegation - without any HTTP involved.
/// </summary>
public sealed class NotifierServiceTests
{
	[Fact]
	public async Task ListAsync_delegates_to_repository_and_returns_result_unchanged()
	{
		var repo = Substitute.For<INotifierRepository>();
		IReadOnlyList<Notifier> notifiers = [new() { Id = "n1", Provider = "slack" }];
		repo.ListAsync(Arg.Any<CancellationToken>()).Returns(notifiers);
		var service = new NotifierService(repo);

		var result = await service.ListAsync(TestContext.Current.CancellationToken);

		Assert.Same(notifiers, result);
		await repo.Received(1).ListAsync(Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task CreateAsync_blank_provider_throws_validation_exception_without_calling_repository(
		string? provider)
	{
		var repo = Substitute.For<INotifierRepository>();
		var service = new NotifierService(repo);
		var arguments = new Dictionary<string, string> { ["to"] = "user@example.com" };

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.CreateAsync(provider!, null, arguments, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().CreateAsync(Arg.Any<CreateNotifierQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateAsync_empty_arguments_throws_validation_exception_without_calling_repository()
	{
		var repo = Substitute.For<INotifierRepository>();
		var service = new NotifierService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() => service.CreateAsync(
			"slack", "desc", new Dictionary<string, string>(), TestContext.Current.CancellationToken));

		await repo.DidNotReceive().CreateAsync(Arg.Any<CreateNotifierQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateAsync_valid_input_builds_query_and_returns_repository_result()
	{
		var repo = Substitute.For<INotifierRepository>();
		var arguments = new Dictionary<string, string> { ["url"] = "https://hooks.slack.com/x" };
		var expected = new CreateNotifierResult { Success = true, Id = "new-id" };
		repo.CreateAsync(Arg.Any<CreateNotifierQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new NotifierService(repo);

		var result =
			await service.CreateAsync("slack", "team alerts", arguments, TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).CreateAsync(
			Arg.Is<CreateNotifierQuery>(q =>
				q.Provider == "slack" && q.Description == "team alerts" && q.Arguments == arguments),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetProvidersAsync_delegates_to_repository_and_returns_result_unchanged()
	{
		var repo = Substitute.For<INotifierRepository>();
		IReadOnlyList<NotifierProvider> providers = [new() { Name = "slack", Required = ["url"] }];
		repo.ListProvidersAsync(Arg.Any<CancellationToken>()).Returns(providers);
		var service = new NotifierService(repo);

		var result = await service.GetProvidersAsync(TestContext.Current.CancellationToken);

		Assert.Same(providers, result);
		await repo.Received(1).ListProvidersAsync(Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task GetAsync_blank_id_throws_validation_exception_without_calling_repository(string? id)
	{
		var repo = Substitute.For<INotifierRepository>();
		var service = new NotifierService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.GetAsync(id!, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetAsync_valid_id_delegates_to_repository_and_returns_result_unchanged()
	{
		var repo = Substitute.For<INotifierRepository>();
		var expected = new Notifier { Id = "n1", Provider = "email" };
		repo.GetAsync("n1", Arg.Any<CancellationToken>()).Returns(expected);
		var service = new NotifierService(repo);

		var result = await service.GetAsync("n1", TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).GetAsync("n1", Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UpdateAsync_blank_id_throws_validation_exception_without_calling_repository()
	{
		var repo = Substitute.For<INotifierRepository>();
		var service = new NotifierService(repo);
		var arguments = new Dictionary<string, string> { ["to"] = "user@example.com" };

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.UpdateAsync(" ", arguments, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().UpdateAsync(Arg.Any<UpdateNotifierQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UpdateAsync_empty_arguments_throws_validation_exception_without_calling_repository()
	{
		var repo = Substitute.For<INotifierRepository>();
		var service = new NotifierService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() => service.UpdateAsync(
			"n1", new Dictionary<string, string>(), TestContext.Current.CancellationToken));

		await repo.DidNotReceive().UpdateAsync(Arg.Any<UpdateNotifierQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UpdateAsync_valid_input_builds_query_and_returns_repository_result()
	{
		var repo = Substitute.For<INotifierRepository>();
		var arguments = new Dictionary<string, string> { ["to"] = "user@example.com" };
		repo.UpdateAsync(Arg.Any<UpdateNotifierQuery>(), Arg.Any<CancellationToken>()).Returns(true);
		var service = new NotifierService(repo);

		var result = await service.UpdateAsync("n1", arguments, TestContext.Current.CancellationToken);

		Assert.True(result);
		await repo.Received(1).UpdateAsync(
			Arg.Is<UpdateNotifierQuery>(q => q.Id == "n1" && q.Arguments == arguments),
			Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task DeleteAsync_blank_id_throws_validation_exception_without_calling_repository(string? id)
	{
		var repo = Substitute.For<INotifierRepository>();
		var service = new NotifierService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.DeleteAsync(id!, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAsync_valid_id_delegates_to_repository_and_returns_result_unchanged()
	{
		var repo = Substitute.For<INotifierRepository>();
		repo.DeleteAsync("n1", Arg.Any<CancellationToken>()).Returns(true);
		var service = new NotifierService(repo);

		var result = await service.DeleteAsync("n1", TestContext.Current.CancellationToken);

		Assert.True(result);
		await repo.Received(1).DeleteAsync("n1", Arg.Any<CancellationToken>());
	}
}

/// <summary>
///     Service-orchestration unit tests for <see cref="DirectoryService" />: the repository is mocked
///     with NSubstitute so these tests verify only the service's own responsibilities - guard-clause
///     validation and delegation - without any HTTP involved.
/// </summary>
public sealed class DirectoryServiceTests
{
	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task ListQueriesAsync_page_below_one_throws_validation_exception_without_calling_repository(
		int page)
	{
		var repo = Substitute.For<IDirectoryRepository>();
		var service = new DirectoryService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.ListQueriesAsync(page, cancellationToken: TestContext.Current.CancellationToken));

		await repo.DidNotReceive().ListQueriesAsync(Arg.Any<ListSavedQueriesQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ListQueriesAsync_valid_input_builds_query_and_returns_repository_result()
	{
		var repo = Substitute.For<IDirectoryRepository>();
		var expected = new SavedQueryResult
		{
			Matches = [new SavedQuery { Query = "port:23" }],
			Total = 1
		};
		repo.ListQueriesAsync(Arg.Any<ListSavedQueriesQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new DirectoryService(repo);

		var result = await service.ListQueriesAsync(2, "votes", "desc", TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).ListQueriesAsync(
			Arg.Is<ListSavedQueriesQuery>(q => q.Page == 2 && q.Sort == "votes" && q.Order == "desc"),
			Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task SearchQueriesAsync_blank_query_throws_validation_exception_without_calling_repository(
		string? query)
	{
		var repo = Substitute.For<IDirectoryRepository>();
		var service = new DirectoryService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.SearchQueriesAsync(query!, cancellationToken: TestContext.Current.CancellationToken));

		await repo.DidNotReceive().SearchQueriesAsync(
			Arg.Any<SearchSavedQueriesQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SearchQueriesAsync_page_below_one_throws_validation_exception_without_calling_repository()
	{
		var repo = Substitute.For<IDirectoryRepository>();
		var service = new DirectoryService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.SearchQueriesAsync("webcam", 0, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().SearchQueriesAsync(
			Arg.Any<SearchSavedQueriesQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SearchQueriesAsync_valid_input_builds_query_and_returns_repository_result()
	{
		var repo = Substitute.For<IDirectoryRepository>();
		var expected = new SavedQueryResult
		{
			Matches = [new SavedQuery { Query = "webcam" }],
			Total = 1
		};
		repo.SearchQueriesAsync(Arg.Any<SearchSavedQueriesQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new DirectoryService(repo);

		var result = await service.SearchQueriesAsync("webcam", 3, TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).SearchQueriesAsync(
			Arg.Is<SearchSavedQueriesQuery>(q => q.Query == "webcam" && q.Page == 3),
			Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-5)]
	public async Task ListTagsAsync_size_below_one_throws_validation_exception_without_calling_repository(int size)
	{
		var repo = Substitute.For<IDirectoryRepository>();
		var service = new DirectoryService(repo);

		await Assert.ThrowsAsync<ShodanRequestValidationException>(() =>
			service.ListTagsAsync(size, TestContext.Current.CancellationToken));

		await repo.DidNotReceive().ListTagsAsync(Arg.Any<ListQueryTagsQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ListTagsAsync_valid_input_builds_query_and_returns_repository_result()
	{
		var repo = Substitute.For<IDirectoryRepository>();
		IReadOnlyList<QueryTag> expected = [new("webcam", 209)];
		repo.ListTagsAsync(Arg.Any<ListQueryTagsQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
		var service = new DirectoryService(repo);

		var result = await service.ListTagsAsync(20, TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).ListTagsAsync(
			Arg.Is<ListQueryTagsQuery>(q => q.Size == 20), Arg.Any<CancellationToken>());
	}
}

/// <summary>
///     Service-orchestration unit tests for <see cref="AccountService" />: the repository is mocked
///     with NSubstitute so these tests verify only the service's own responsibility - delegation -
///     without any HTTP involved. <see cref="AccountService" /> has no guard clauses to validate.
/// </summary>
public sealed class AccountServiceTests
{
	[Fact]
	public async Task GetProfileAsync_delegates_to_repository_and_returns_result_unchanged()
	{
		var repo = Substitute.For<IAccountRepository>();
		var expected = new AccountProfile { Member = true, Credits = 100, DisplayName = "test" };
		repo.GetProfileAsync(Arg.Any<CancellationToken>()).Returns(expected);
		var service = new AccountService(repo);

		var result = await service.GetProfileAsync(TestContext.Current.CancellationToken);

		Assert.Same(expected, result);
		await repo.Received(1).GetProfileAsync(Arg.Any<CancellationToken>());
	}
}
