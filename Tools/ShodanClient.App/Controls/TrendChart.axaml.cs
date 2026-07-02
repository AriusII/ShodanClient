using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using AvaloniaApplication = Avalonia.Application;

namespace ShodanClient.App.Controls;

/// <summary>
///     A minimal, hand-drawn month-by-month line chart (no charting package). Used by Trends and any
///     other module plotting a simple historical count series.
/// </summary>
public partial class TrendChart : UserControl
{
	private const double TopMargin = 16;
	private const double BottomMargin = 18;
	private const double MaxAxisLabels = 5;

	/// <summary>Defines the <see cref="ItemsSource" /> property.</summary>
	public static readonly StyledProperty<IReadOnlyList<TrendPoint>?> ItemsSourceProperty =
		AvaloniaProperty.Register<TrendChart, IReadOnlyList<TrendPoint>?>(nameof(ItemsSource));

	/// <summary>Defines the <see cref="Stroke" /> property.</summary>
	public static readonly StyledProperty<IBrush?> StrokeProperty =
		AvaloniaProperty.Register<TrendChart, IBrush?>(nameof(Stroke));

	/// <summary>Creates the control.</summary>
	public TrendChart()
	{
		InitializeComponent();
		PropertyChanged += OnAnyPropertyChanged;
	}

	/// <summary>The month/count series to plot, in chronological order.</summary>
	public IReadOnlyList<TrendPoint>? ItemsSource
	{
		get => GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	/// <summary>
	///     The plotted line's color. Defaults to the app's <c>ShodanAccentBrush</c> theme resource so
	///     the Trends module matches the rest of the app's brand accent instead of a stock system color.
	/// </summary>
	public IBrush? Stroke
	{
		get => GetValue(StrokeProperty);
		set => SetValue(StrokeProperty, value);
	}

	private void OnAnyPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
	{
		if (e.Property == ItemsSourceProperty || e.Property == BoundsProperty || e.Property == StrokeProperty)
		{
			Redraw();
		}
	}

	private void Redraw()
	{
		var canvas = this.FindControl<Canvas>("ChartCanvas");
		if (canvas is null)
		{
			return;
		}

		canvas.Children.Clear();

		var points = ItemsSource;
		var width = Bounds.Width;
		var height = Bounds.Height;
		if (points is not { Count: > 0 } || width <= 0 || height <= 0)
		{
			return;
		}

		var max = points.Max(p => p.Count);
		if (max <= 0)
		{
			max = 1;
		}

		var stroke = Stroke ?? ResolveAccentBrush();
		var labelBrush = Brushes.Gray;
		var labelFontSize = ResolveLabelFontSize();

		var plotHeight = Math.Max(height - TopMargin - BottomMargin, 4);
		var stepX = points.Count > 1 ? width / (points.Count - 1) : 0;
		var linePoints = new List<Point>(points.Count);
		for (var i = 0; i < points.Count; i++)
		{
			var x = points.Count > 1 ? i * stepX : width / 2;
			var y = TopMargin + plotHeight - plotHeight * ((double)points[i].Count / max);
			linePoints.Add(new Point(x, y));
		}

		var polyline = new Polyline
		{
			Points = linePoints,
			Stroke = stroke,
			StrokeThickness = 2,
			StrokeJoin = PenLineJoin.Round
		};
		canvas.Children.Add(polyline);

		// Y-axis scale: just the max value, top-left — enough to read the plotted magnitude without
		// cluttering a chart this small with a full tick ladder.
		AddLabel(canvas, max.ToString("N0"), 0, 0, labelBrush, labelFontSize);

		// Per-point hit-test marker (with a "{Month}: {Count}" tooltip) plus x-axis month labels for
		// a representative subset of points (first, last, and a few evenly spaced in between) so
		// labels never overlap on a series with many points.
		var labelIndices = SelectLabelIndices(points.Count);
		for (var i = 0; i < points.Count; i++)
		{
			var point = points[i];
			var position = linePoints[i];

			var marker = new Ellipse
			{
				Width = 6,
				Height = 6,
				Fill = stroke
			};
			ToolTip.SetTip(marker, $"{point.Month}: {point.Count:N0}");
			Canvas.SetLeft(marker, position.X - 3);
			Canvas.SetTop(marker, position.Y - 3);
			canvas.Children.Add(marker);

			if (labelIndices.Contains(i))
			{
				AddCenteredLabel(canvas, point.Month, position.X, height - BottomMargin + 2, labelBrush,
					labelFontSize, width);
			}
		}
	}

	/// <summary>Picks up to <see cref="MaxAxisLabels" /> evenly spaced indices, always including the first and last.</summary>
	private static HashSet<int> SelectLabelIndices(int count)
	{
		var labelCount = Math.Min(count, (int)MaxAxisLabels);
		var indices = new HashSet<int>();
		if (labelCount <= 1)
		{
			indices.Add(0);
			return indices;
		}

		for (var i = 0; i < labelCount; i++)
		{
			indices.Add((int)Math.Round(i * (count - 1) / (double)(labelCount - 1)));
		}

		return indices;
	}

	private static void AddLabel(Canvas canvas, string text, double x, double y, IBrush brush, double fontSize)
	{
		var label = new TextBlock { Text = text, FontSize = fontSize, Foreground = brush };
		Canvas.SetLeft(label, x);
		Canvas.SetTop(label, y);
		canvas.Children.Add(label);
	}

	private static void AddCenteredLabel(Canvas canvas, string text, double centerX, double y, IBrush brush,
		double fontSize, double canvasWidth)
	{
		var label = new TextBlock { Text = text, FontSize = fontSize, Foreground = brush };
		label.Measure(Size.Infinity);
		var x = Math.Clamp(centerX - label.DesiredSize.Width / 2, 0,
			Math.Max(0, canvasWidth - label.DesiredSize.Width));
		Canvas.SetLeft(label, x);
		Canvas.SetTop(label, y);
		canvas.Children.Add(label);
	}

	private static IBrush ResolveAccentBrush()
	{
		if (AvaloniaApplication.Current?.TryFindResource("ShodanAccentBrush", out var resource) == true &&
			resource is IBrush brush)
		{
			return brush;
		}

		return Brushes.OrangeRed;
	}

	private static double ResolveLabelFontSize()
	{
		if (AvaloniaApplication.Current?.TryFindResource("ShodanFontSizeSmall", out var resource) == true &&
			resource is double size)
		{
			return size;
		}

		return 11;
	}
}
