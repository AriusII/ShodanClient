using System.Text.Json.Serialization;
using ShodanClient.Infrastructure.Account.Wire;
using ShodanClient.Infrastructure.Alerts.Wire;
using ShodanClient.Infrastructure.ApiStatus.Wire;
using ShodanClient.Infrastructure.BulkData.Wire;
using ShodanClient.Infrastructure.Directory.Wire;
using ShodanClient.Infrastructure.Dns.Wire;
using ShodanClient.Infrastructure.Exploits.Wire;
using ShodanClient.Infrastructure.InternetDb.Wire;
using ShodanClient.Infrastructure.Notifiers.Wire;
using ShodanClient.Infrastructure.Organization.Wire;
using ShodanClient.Infrastructure.Scanning.Wire;
using ShodanClient.Infrastructure.Search.Wire;
using ShodanClient.Infrastructure.Trends.Wire;

namespace ShodanClient.Infrastructure.Serialization;

/// <summary>
///     The single source-generated <see cref="JsonSerializerContext" /> for every Shodan wire DTO.
///     Source generation keeps (de)serialization allocation-light, reflection-free and Native-AOT/trim
///     safe. Snake-case naming matches Shodan's field names; unknown fields survive via each DTO's
///     <c>[JsonExtensionData]</c> bag; numbers may arrive as quoted strings.
/// </summary>
/// <remarks>
///     Each bounded context registers its wire types by adding <c>[JsonSerializable(typeof(...))]</c>
///     attributes to this partial class (grouped below by context). Metadata generation mode is
///     required because DTOs use <c>[JsonExtensionData]</c> and relaxed number handling.
/// </remarks>
[JsonSourceGenerationOptions(
	PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	NumberHandling = JsonNumberHandling.AllowReadingFromString,
	UseStringEnumConverter = true,
	AllowDuplicateProperties = false,
	GenerationMode = JsonSourceGenerationMode.Metadata)]

// --- Cross-cutting -----------------------------------------------------------------------------
[JsonSerializable(typeof(ShodanErrorResponse))]
[JsonSerializable(typeof(ShodanSuccessResponse))]

// --- Bare collection / scalar payloads ---------------------------------------------------------
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(int[]))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]

// --- Search ------------------------------------------------------------------------------------
[JsonSerializable(typeof(HostResponse))]
[JsonSerializable(typeof(SearchResponse))]
[JsonSerializable(typeof(CountResponse))]
[JsonSerializable(typeof(TokenParseResponse))]

// --- Account -----------------------------------------------------------------------------------
[JsonSerializable(typeof(AccountProfileResponse))]

// --- API Status --------------------------------------------------------------------------------
[JsonSerializable(typeof(ApiInfoResponse))]

// --- DNS ---------------------------------------------------------------------------------------
[JsonSerializable(typeof(DomainDnsResponse))]

// --- Scanning ----------------------------------------------------------------------------------
[JsonSerializable(typeof(ScanSubmissionResponse))]
[JsonSerializable(typeof(ScanStatusResponse))]
[JsonSerializable(typeof(ScanListResponse))]
[JsonSerializable(typeof(ScanInternetResponse))]

// --- Directory ---------------------------------------------------------------------------------
[JsonSerializable(typeof(SavedQueryResponse))]
[JsonSerializable(typeof(QueryTagsResponse))]

// --- Notifiers ---------------------------------------------------------------------------------
[JsonSerializable(typeof(NotifierListResponse))]
[JsonSerializable(typeof(NotifierDto))]
[JsonSerializable(typeof(CreateNotifierResponse))]
[JsonSerializable(typeof(Dictionary<string, NotifierProviderDto>))]

// --- Organization ------------------------------------------------------------------------------
[JsonSerializable(typeof(OrganizationResponse))]

// --- Bulk Data ---------------------------------------------------------------------------------
[JsonSerializable(typeof(DatasetDto[]))]
[JsonSerializable(typeof(DatasetFileDto[]))]

// --- Alerts ------------------------------------------------------------------------------------
[JsonSerializable(typeof(AlertDto))]
[JsonSerializable(typeof(AlertDto[]))]
[JsonSerializable(typeof(TriggerDefinitionDto[]))]
[JsonSerializable(typeof(CreateAlertPayload))]
[JsonSerializable(typeof(UpdateAlertPayload))]

// --- InternetDB ----------------------------------------------------------------------------------
[JsonSerializable(typeof(InternetDbResponse))]

// --- Trends --------------------------------------------------------------------------------------
[JsonSerializable(typeof(TrendSearchResponse))]

// --- Exploits --------------------------------------------------------------------------------------
[JsonSerializable(typeof(ExploitSearchResponse))]
[JsonSerializable(typeof(ExploitCountResponse))]

// --- Streaming (reuses Search.Wire.BannerDto, already registered above) --------------------------
internal sealed partial class ShodanJsonContext : JsonSerializerContext;
