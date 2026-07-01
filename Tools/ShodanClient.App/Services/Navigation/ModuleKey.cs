namespace ShodanClient.App.Services.Navigation;

/// <summary>Identifies a navigable top-level module of the shell.</summary>
public enum ModuleKey
{
	/// <summary>Account/plan summary and quick InternetDB lookup.</summary>
	Dashboard,

	/// <summary>Shodan search builder, results grid and facets.</summary>
	Search,

	/// <summary>Manual host lookup (paid <c>Hosts</c> vs free InternetDB).</summary>
	HostLookup,

	/// <summary>Live banner streaming.</summary>
	Streaming,

	/// <summary>Network alert CRUD and triggers.</summary>
	Alerts,

	/// <summary>Notifier CRUD.</summary>
	Notifiers,

	/// <summary>On-demand scans and crawl status.</summary>
	Scans,

	/// <summary>Directory of saved search queries.</summary>
	Directory,

	/// <summary>DNS lookup/resolve/reverse tools.</summary>
	Dns,

	/// <summary>Historical search trends.</summary>
	Trends,

	/// <summary>Exploit/vulnerability search.</summary>
	Exploits,

	/// <summary>Organization management (Enterprise).</summary>
	Organization,

	/// <summary>Bulk data datasets (Enterprise).</summary>
	BulkData,

	/// <summary>My-IP, HTTP headers, raw usage limits.</summary>
	Diagnostics,

	/// <summary>Local app settings: API key rotation, theme, resilience overrides.</summary>
	Settings,

	/// <summary>First-run/invalid-key onboarding gate.</summary>
	Setup
}
