using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShodanClient.App.Services.Notifications;
using ShodanClient.App.Services.ShodanClientAccessor;
using ShodanClient.App.Session;
using ShodanClient.Application.Exceptions;
using ShodanClient.Domain.BulkData;

namespace ShodanClient.App.ViewModels.BulkData;

/// <summary>
///     Enterprise bulk-data datasets and their downloadable files (<c>GET /shodan/data*</c>).
/// </summary>
public sealed partial class BulkDataViewModel : ModuleViewModelBase
{
	private readonly INotificationService _notifications;
	private readonly IPlanCapabilities _planCapabilities;

	/// <summary>Creates the Bulk Data module view model and kicks off the initial dataset load.</summary>
	/// <param name="planCapabilities">
	///     The shared, cross-module Enterprise-capability cache (also consumed by <c>ShellViewModel</c>
	///     to disable/gray out this module's nav item once <see cref="IPlanCapabilities.BulkDataAccessDenied" />
	///     flips).
	/// </param>
	public BulkDataViewModel(
		INotificationService notifications,
		IShodanClientAccessor accessor,
		IPlanCapabilities planCapabilities)
		: base(notifications)
	{
		_notifications = notifications;
		Accessor = accessor;
		_planCapabilities = planCapabilities;
		Title = "Bulk Data";

		// The shared, cross-module flag may already be known from an earlier 403 this session (e.g.
		// Organization already confirmed the account isn't on an Enterprise plan); reflect that
		// immediately instead of waiting for a fresh, doomed request to confirm it again.
		RequiresEnterprise = _planCapabilities.BulkDataAccessDenied;

		Files.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoFilesForSelectedDataset));

		LoadDatasetsCommand.Execute(null);
	}

	/// <summary>
	///     Set when <c>GET /shodan/data</c> responded with a 403: the calling account isn't on an
	///     Enterprise plan. This is an expected, common outcome for non-Enterprise users, not a bug,
	///     so it is surfaced as a local empty-state rather than a generic error toast.
	/// </summary>
	[ObservableProperty]
	public partial bool RequiresEnterprise { get; set; }

	/// <summary>The currently selected dataset, if any.</summary>
	[ObservableProperty]
	public partial Dataset? SelectedDataset { get; set; }

	/// <summary>The active Shodan client accessor.</summary>
	public IShodanClientAccessor Accessor { get; }

	/// <summary>The datasets available for bulk download.</summary>
	public ObservableCollection<Dataset> Datasets { get; } = [];

	/// <summary>The files within <see cref="SelectedDataset" />, if one is selected.</summary>
	public ObservableCollection<DatasetFile> Files { get; } = [];

	/// <summary>Whether a dataset is selected but genuinely has zero files, for the Files panel's empty state.</summary>
	public bool HasNoFilesForSelectedDataset => SelectedDataset is not null && Files.Count == 0;

	partial void OnSelectedDatasetChanged(Dataset? value)
	{
		Files.Clear();
		if (value is not null)
		{
			LoadFilesCommand.Execute(value.Name);
		}

		OnPropertyChanged(nameof(HasNoFilesForSelectedDataset));
	}

	/// <summary>Reloads the list of available datasets.</summary>
	[RelayCommand]
	private Task LoadDatasetsAsync(CancellationToken cancellationToken) => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client)
		{
			return;
		}

		Datasets.Clear();
		Files.Clear();
		SelectedDataset = null;

		if (_planCapabilities.BulkDataAccessDenied)
		{
			// Already confirmed denied earlier this session (possibly by Organization's matching
			// Enterprise gate); short-circuit straight to the empty state instead of re-issuing a
			// request that's near-certain to 403 again.
			RequiresEnterprise = true;
			return;
		}

		try
		{
			var datasets = await client.BulkData.ListDatasetsAsync(ct).ConfigureAwait(true);
			foreach (var dataset in datasets)
			{
				Datasets.Add(dataset);
			}

			RequiresEnterprise = false;
			_planCapabilities.BulkDataAccessDenied = false;
		}
		catch (ShodanAccessDeniedException)
		{
			RequiresEnterprise = true;
			_planCapabilities.BulkDataAccessDenied = true;
		}
	}, cancellationToken);

	/// <summary>Loads the files within the named dataset.</summary>
	[RelayCommand]
	private Task LoadFilesAsync(string? dataset, CancellationToken cancellationToken) => RunAsync(async ct =>
	{
		if (Accessor.Client is not { } client || string.IsNullOrWhiteSpace(dataset))
		{
			return;
		}

		Files.Clear();
		var files = await client.BulkData.ListFilesAsync(dataset, ct).ConfigureAwait(true);
		foreach (var file in files)
		{
			Files.Add(file);
		}
	}, cancellationToken);

	/// <summary>Opens a file's download URL in the system's default browser.</summary>
	[RelayCommand]
	private void OpenDownloadUrl(DatasetFile? file)
	{
		var url = file?.Url;
		if (url is null)
		{
			return;
		}

		try
		{
			Process.Start(new ProcessStartInfo(url.ToString()) { UseShellExecute = true });
		}
		catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
		{
			_notifications.Warning($"Couldn't open the download link: {ex.Message}");
		}
	}
}
