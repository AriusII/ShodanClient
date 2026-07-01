using ShodanClient.Infrastructure.Directory.Mapping;
using ShodanClient.Infrastructure.Directory.Wire;
using ShodanClient.Infrastructure.Notifiers.Mapping;
using ShodanClient.Infrastructure.Notifiers.Wire;
using ShodanClient.Infrastructure.Scanning.Mapping;
using ShodanClient.Infrastructure.Scanning.Wire;

namespace ShodanClient.Infrastructure.Tests;

/// <summary>
///     Pure unit tests for the Scanning, Notifiers and Directory mapping extension methods: wire DTO in,
///     domain model out, no HTTP/DI involved.
/// </summary>
public sealed class ScanningNotifiersDirectoryMapperTests
{
	// ----- ScanMapper -----------------------------------------------------

	[Fact]
	public void ScanSubmissionResponse_ToDomain_maps_all_fields()
	{
		var dto = new ScanSubmissionResponse { Id = "scan-1", Count = 5, CreditsLeft = 95 };

		var result = dto.ToDomain();

		Assert.Equal("scan-1", result.Id);
		Assert.Equal(5, result.Count);
		Assert.Equal(95, result.CreditsLeft);
	}

	[Fact]
	public void ScanSubmissionResponse_ToDomain_null_id_defaults_to_empty_string()
	{
		var dto = new ScanSubmissionResponse { Id = null, Count = 0, CreditsLeft = 0 };

		var result = dto.ToDomain();

		Assert.Equal(string.Empty, result.Id);
	}

	[Fact]
	public void ScanInternetResponse_ToDomain_maps_id()
	{
		var dto = new ScanInternetResponse { Id = "internet-scan-1" };

		var result = dto.ToDomain();

		Assert.Equal("internet-scan-1", result.Id);
	}

	[Fact]
	public void ScanInternetResponse_ToDomain_null_id_defaults_to_empty_string()
	{
		var dto = new ScanInternetResponse { Id = null };

		var result = dto.ToDomain();

		Assert.Equal(string.Empty, result.Id);
	}

	[Fact]
	public void ScanStatusResponse_ToDomain_maps_all_fields_and_parses_timestamp()
	{
		var dto = new ScanStatusResponse
		{
			Id = "scan-42",
			Status = "PROCESSING",
			Created = "2024-09-29T09:39:45.813661",
			Count = 12
		};

		var result = dto.ToDomain();

		Assert.Equal("scan-42", result.Id);
		Assert.Equal("PROCESSING", result.Status);
		Assert.Equal(12, result.Count);
		Assert.NotNull(result.Created);
		var expectedCreated = new DateTimeOffset(2024, 9, 29, 9, 39, 45, TimeSpan.Zero).AddTicks(8136610);
		Assert.Equal(expectedCreated, result.Created!.Value);
		Assert.Equal(TimeSpan.Zero, result.Created!.Value.Offset);
	}

	[Fact]
	public void ScanStatusResponse_ToDomain_missing_optional_fields_use_defaults()
	{
		var dto = new ScanStatusResponse { Id = null, Status = null, Created = null, Count = 0 };

		var result = dto.ToDomain();

		Assert.Equal(string.Empty, result.Id);
		Assert.Equal(string.Empty, result.Status);
		Assert.Null(result.Created);
	}

	[Fact]
	public void ScanListResponse_ToDomain_maps_matches()
	{
		var dto = new ScanListResponse
		{
			Total = 2,
			Matches =
			[
				new ScanListEntryDto
				{
					Id = "s1", Created = "2023-01-15T10:00:00", Status = "DONE", Size = 10, CreditsLeft = 90
				},
				new ScanListEntryDto
				{
					Id = "s2", Created = null, Status = "QUEUE", Size = 3, CreditsLeft = 87
				}
			]
		};

		var result = dto.ToDomain();

		Assert.Equal(2, result.Count);
		Assert.Equal("s1", result[0].Id);
		Assert.Equal("DONE", result[0].Status);
		Assert.Equal(10, result[0].Size);
		Assert.Equal(90, result[0].CreditsLeft);
		Assert.NotNull(result[0].Created);
		Assert.Equal(new DateOnly(2023, 1, 15), DateOnly.FromDateTime(result[0].Created!.Value.UtcDateTime));
		Assert.Equal("s2", result[1].Id);
		Assert.Null(result[1].Created);
	}

	[Fact]
	public void ScanListResponse_ToDomain_null_matches_returns_empty_list()
	{
		var dto = new ScanListResponse { Total = 0, Matches = null };

		var result = dto.ToDomain();

		Assert.Empty(result);
	}

	[Fact]
	public void ScanListResponse_ToDomain_empty_matches_returns_empty_list()
	{
		var dto = new ScanListResponse { Total = 0, Matches = [] };

		var result = dto.ToDomain();

		Assert.Empty(result);
	}

	// ----- NotifierMapper ---------------------------------------------------

	[Fact]
	public void NotifierDto_ToDomain_maps_all_fields()
	{
		var dto = new NotifierDto
		{
			Id = "notifier-1",
			Provider = "slack",
			Description = "Team alerts",
			Args = new Dictionary<string, string> { ["url"] = "https://hooks.slack.com/x" }
		};

		var result = dto.ToDomain();

		Assert.Equal("notifier-1", result.Id);
		Assert.Equal("slack", result.Provider);
		Assert.Equal("Team alerts", result.Description);
		Assert.Equal("https://hooks.slack.com/x", result.Args["url"]);
	}

	[Fact]
	public void NotifierDto_ToDomain_missing_optional_fields_use_defaults()
	{
		var dto = new NotifierDto { Id = null, Provider = null, Description = null, Args = null };

		var result = dto.ToDomain();

		Assert.Equal(string.Empty, result.Id);
		Assert.Equal(string.Empty, result.Provider);
		Assert.Null(result.Description);
		Assert.NotNull(result.Args);
		Assert.Empty(result.Args);
	}

	[Fact]
	public void NotifierListResponse_ToDomain_maps_matches()
	{
		var dto = new NotifierListResponse
		{
			Total = 2,
			Matches =
			[
				new NotifierDto { Id = "n1", Provider = "email", Args = new Dictionary<string, string>() },
				new NotifierDto { Id = "n2", Provider = "webhook", Args = new Dictionary<string, string>() }
			]
		};

		var result = dto.ToDomain();

		Assert.Equal(2, result.Count);
		Assert.Equal("n1", result[0].Id);
		Assert.Equal("n2", result[1].Id);
	}

	[Fact]
	public void NotifierListResponse_ToDomain_null_matches_returns_empty_list()
	{
		var dto = new NotifierListResponse { Total = 0, Matches = null };

		var result = dto.ToDomain();

		Assert.Empty(result);
	}

	[Fact]
	public void CreateNotifierResponse_ToDomain_maps_success_and_id()
	{
		var dto = new CreateNotifierResponse { Success = true, Id = "new-notifier" };

		var result = dto.ToDomain();

		Assert.True(result.Success);
		Assert.Equal("new-notifier", result.Id);
	}

	[Fact]
	public void CreateNotifierResponse_ToDomain_maps_failure_with_null_id()
	{
		var dto = new CreateNotifierResponse { Success = false, Id = null };

		var result = dto.ToDomain();

		Assert.False(result.Success);
		Assert.Null(result.Id);
	}

	[Fact]
	public void NotifierProviderDictionary_ToDomain_maps_keys_to_names_and_required_lists()
	{
		var dto = new Dictionary<string, NotifierProviderDto>
		{
			["slack"] = new() { Required = ["url"] },
			["email"] = new() { Required = ["to", "from"] }
		};

		var result = dto.ToDomain();

		Assert.Equal(2, result.Count);
		var slack = Assert.Single(result, p => p.Name == "slack");
		Assert.Equal(["url"], slack.Required);
		var email = Assert.Single(result, p => p.Name == "email");
		Assert.Equal(["to", "from"], email.Required);
	}

	[Fact]
	public void NotifierProviderDictionary_ToDomain_null_required_defaults_to_empty_list()
	{
		var dto = new Dictionary<string, NotifierProviderDto> { ["custom"] = new() { Required = null } };

		var result = dto.ToDomain();

		var provider = Assert.Single(result);
		Assert.Equal("custom", provider.Name);
		Assert.Empty(provider.Required);
	}

	[Fact]
	public void NotifierProviderDictionary_ToDomain_empty_dictionary_returns_empty_list()
	{
		var dto = new Dictionary<string, NotifierProviderDto>();

		var result = dto.ToDomain();

		Assert.Empty(result);
	}

	// ----- DirectoryMapper --------------------------------------------------

	[Fact]
	public void SavedQueryDto_ToDomain_maps_all_fields_and_parses_timestamp()
	{
		var dto = new SavedQueryDto
		{
			Query = "port:23 country:\"CN\"",
			Votes = 42,
			Title = "Open Telnet",
			Description = "Devices with an open telnet port",
			Timestamp = "2020-09-29T09:39:45.813661",
			Tags = ["telnet", "iot"]
		};

		var result = dto.ToDomain();

		Assert.Equal("port:23 country:\"CN\"", result.Query);
		Assert.Equal(42, result.Votes);
		Assert.Equal("Open Telnet", result.Title);
		Assert.Equal("Devices with an open telnet port", result.Description);
		Assert.NotNull(result.Timestamp);
		var expectedTimestamp = new DateTimeOffset(2020, 9, 29, 9, 39, 45, TimeSpan.Zero).AddTicks(8136610);
		Assert.Equal(expectedTimestamp, result.Timestamp!.Value);
		Assert.Equal(["telnet", "iot"], result.Tags);
	}

	[Fact]
	public void SavedQueryDto_ToDomain_missing_optional_fields_use_defaults()
	{
		var dto = new SavedQueryDto
		{
			Query = null,
			Votes = 0,
			Title = null,
			Description = null,
			Timestamp = null,
			Tags = null
		};

		var result = dto.ToDomain();

		Assert.Equal(string.Empty, result.Query);
		Assert.Null(result.Title);
		Assert.Null(result.Description);
		Assert.Null(result.Timestamp);
		Assert.NotNull(result.Tags);
		Assert.Empty(result.Tags);
	}

	[Fact]
	public void SavedQueryDto_ToDomain_unparseable_timestamp_yields_null()
	{
		var dto = new SavedQueryDto { Query = "q", Timestamp = "not-a-date" };

		var result = dto.ToDomain();

		Assert.Null(result.Timestamp);
	}

	[Fact]
	public void SavedQueryResponse_ToDomain_maps_matches_and_total()
	{
		var dto = new SavedQueryResponse
		{
			Total = 2,
			Matches =
			[
				new SavedQueryDto { Query = "q1" },
				new SavedQueryDto { Query = "q2" }
			]
		};

		var result = dto.ToDomain();

		Assert.Equal(2, result.Total);
		Assert.Equal(2, result.Matches.Count);
		Assert.Equal("q1", result.Matches[0].Query);
		Assert.Equal("q2", result.Matches[1].Query);
	}

	[Fact]
	public void SavedQueryResponse_ToDomain_null_matches_returns_empty_list()
	{
		var dto = new SavedQueryResponse { Total = 0, Matches = null };

		var result = dto.ToDomain();

		Assert.Empty(result.Matches);
		Assert.Equal(0, result.Total);
	}

	[Fact]
	public void QueryTagDto_ToDomain_maps_value_and_count()
	{
		var dto = new QueryTagDto { Value = "webcam", Count = 209 };

		var result = dto.ToDomain();

		Assert.Equal("webcam", result.Value);
		Assert.Equal(209, result.Count);
	}

	[Fact]
	public void QueryTagDto_ToDomain_null_value_defaults_to_empty_string()
	{
		var dto = new QueryTagDto { Value = null, Count = 0 };

		var result = dto.ToDomain();

		Assert.Equal(string.Empty, result.Value);
	}

	[Fact]
	public void QueryTagsResponse_ToDomain_maps_matches()
	{
		var dto = new QueryTagsResponse
		{
			Total = 2,
			Matches =
			[
				new QueryTagDto { Value = "webcam", Count = 209 },
				new QueryTagDto { Value = "default password", Count = 130 }
			]
		};

		var result = dto.ToDomain();

		Assert.Equal(2, result.Count);
		Assert.Equal("webcam", result[0].Value);
		Assert.Equal(209, result[0].Count);
		Assert.Equal("default password", result[1].Value);
	}

	[Fact]
	public void QueryTagsResponse_ToDomain_null_matches_returns_empty_list()
	{
		var dto = new QueryTagsResponse { Total = 0, Matches = null };

		var result = dto.ToDomain();

		Assert.Empty(result);
	}
}
