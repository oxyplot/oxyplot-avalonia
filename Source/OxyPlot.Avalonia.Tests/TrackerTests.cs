using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using OxyPlot;
using OxyPlot.Avalonia;
using CoreSeries = OxyPlot.Series;
using ShapePath = Avalonia.Controls.Shapes.Path;

namespace OxyPlot.Avalonia.Tests;

/// <summary>
/// Tracker tests: the template from the theme, the bindings and the shape of the border.
/// </summary>
public class TrackerTests
{
    [AvaloniaFact]
    public void PlotView_ShowTrackerInDarkTheme_TextIsBlackAndBound()
    {
        var model = new PlotModel();
        model.Series.Add(new CoreSeries.FunctionSeries(Math.Sin, 0, 10, 0.1));
        var view = new PlotView { Model = model };
        var window = Ui.Show(view, ThemeVariant.Dark);

        view.ShowTracker(new TrackerHitResult { Text = "value", Position = new ScreenPoint(50, 60), PlotModel = model });
        Ui.Flush();

        var tracker = Assert.Single(view.GetVisualDescendants().OfType<TrackerControl>());
        Assert.Equal(new ScreenPoint(50, 60), tracker.Position);
        Assert.Equal(model.PlotArea, tracker.LineExtents);

        // the tracker background is always light: in a dark theme the inherited white text is not readable on it
        var text = Assert.Single(tracker.GetVisualDescendants().OfType<TextBlock>(), t => t.Text == "value");
        Assert.Equal(Colors.Black, Assert.IsAssignableFrom<ISolidColorBrush>(text.Foreground).Color);
        window.Close();
    }

    [AvaloniaFact]
    public void TrackerControl_LineDashArray_ReachesBothLines()
    {
        var tracker = new TrackerControl { LineDashArray = new AvaloniaList<double> { 2, 1 } };
        var window = Ui.Show(tracker);

        var lines = tracker.GetVisualDescendants().OfType<Line>()
            .Where(l => l.Name is "PART_HorizontalLine" or "PART_VerticalLine")
            .ToList();

        Assert.Equal(2, lines.Count);
        Assert.All(lines, l => Assert.Equal(new[] { 2.0, 1.0 }, l.StrokeDashArray!));
        window.Close();
    }

    [AvaloniaFact]
    public void TrackerControl_CornerRadiusWithoutPointer_RoundsBorder()
    {
        var tracker = new TrackerControl
        {
            ShowPointer = false,
            CornerRadius = 6,
            Content = new TextBlock { Text = "value", Margin = new Thickness(7) },
        };
        var canvas = new Canvas();
        canvas.Children.Add(tracker);
        var window = Ui.Show(canvas);

        tracker.Position = new ScreenPoint(100, 100);
        Ui.Flush();

        var path = Assert.Single(tracker.GetVisualDescendants().OfType<ShapePath>(), p => p.Name == "PART_Path");
        var geometry = Assert.IsType<RectangleGeometry>(path.Data);
        Assert.Equal(6, geometry.RadiusX);
        Assert.Equal(6, geometry.RadiusY);
        window.Close();
    }

    [AvaloniaFact]
    public void TrackerControlSubclass_Shown_GetsTemplate()
    {
        var tracker = new DerivedTrackerControl();

        var window = Ui.Show(tracker);

        Assert.NotNull(tracker.Template);
        window.Close();
    }

    /// <summary>
    /// A descendant of the tracker: styles with an exact type selector did not find it.
    /// </summary>
    private sealed class DerivedTrackerControl : TrackerControl
    {
    }
}
