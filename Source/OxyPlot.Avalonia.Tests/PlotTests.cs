using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using OxyPlot;
using OxyPlot.Avalonia;
using CoreAnnotations = OxyPlot.Annotations;
using CoreAxes = OxyPlot.Axes;
using CoreSeries = OxyPlot.Series;

namespace OxyPlot.Avalonia.Tests;

/// <summary>
/// Tests that plots are built and rendered under Avalonia 12.
/// </summary>
public class PlotTests
{
    [AvaloniaFact]
    public void PlotView_WithModel_RendersPlotArea()
    {
        var model = new PlotModel { Title = "Plot" };
        model.Series.Add(new CoreSeries.FunctionSeries(Math.Sin, 0, 10, 0.1));
        var view = new PlotView { Model = model };

        var window = Ui.Show(view);

        // the plot area is computed while the model is rendered: so the template is applied and the model is drawn
        Assert.Same(model, view.ActualModel);
        Assert.True(model.PlotArea.Width > 0);

        // the canvas contains the title and the line of the function, not just the computed area
        Assert.Contains(view.GetVisualDescendants().OfType<TextBlock>(), t => t.Text == "Plot");
        Assert.Contains(view.GetVisualDescendants().OfType<Polyline>(), p => p.Points.Count > 10);
        window.Close();
    }

    [AvaloniaFact]
    public void Plot_WithLineSeries_BuildsModelFromItemsSource()
    {
        var points = new[] { new DataPoint(0, 0), new DataPoint(1, 1), new DataPoint(2, 4) };
        var plot = new Plot();
        plot.Series.Add(new LineSeries { ItemsSource = points });

        var window = Ui.Show(plot);

        // the series control copies its properties to the series of the OxyPlot model
        var series = Assert.Single(plot.ActualModel.Series);
        Assert.Equal(points, Assert.IsType<CoreSeries.LineSeries>(series).ItemsSource.Cast<DataPoint>());
        Assert.True(plot.ActualModel.PlotArea.Width > 0);
        Assert.Contains(Ui.PlotCanvas(plot).Children.OfType<Polyline>(), p => p.Points.Count == points.Length);
        window.Close();
    }

    [AvaloniaFact]
    public void Plot_AddSeriesAfterAxes_LoadsDataAndRangesAxes()
    {
        // the axes are added before the series: the first model update ran without data,
        // and adding a series must load its points, not just redraw
        var plot = new Plot();
        plot.Axes.Add(new LinearAxis { Position = CoreAxes.AxisPosition.Bottom });
        plot.Axes.Add(new LinearAxis { Position = CoreAxes.AxisPosition.Left });
        var window = Ui.Show(plot);

        plot.Series.Add(new LineSeries { ItemsSource = new[] { new DataPoint(0, 0), new DataPoint(10, 100), new DataPoint(20, 50) } });
        Ui.Flush();

        var series = Assert.IsAssignableFrom<CoreSeries.XYAxisSeries>(Assert.Single(plot.ActualModel.Series));
        Assert.Equal(100, series.MaxY);
        Assert.True(plot.ActualModel.Axes[0].ActualMaximum >= 20);
        Assert.True(plot.ActualModel.Axes[1].ActualMaximum >= 100);
        window.Close();
    }

    [AvaloniaFact]
    public void Plot_ShowHiddenSeries_LoadsItsData()
    {
        var visible = new LineSeries { ItemsSource = new[] { new DataPoint(0, 0), new DataPoint(1, 4) } };
        var hidden = new LineSeries { ItemsSource = new[] { new DataPoint(0, 0), new DataPoint(1, 30) }, IsVisible = false };
        var plot = new Plot();
        plot.Series.Add(visible);
        plot.Series.Add(hidden);
        var window = Ui.Show(plot);

        hidden.IsVisible = true;
        Ui.Flush();

        // the core loads the data of visible series only, and only when the data is updated
        Assert.Equal(30, ((CoreSeries.XYAxisSeries)hidden.InternalSeries).MaxY);
        Assert.True(plot.ActualModel.DefaultYAxis.ActualMaximum >= 30);
        window.Close();
    }

    [AvaloniaFact]
    public void Plot_HideSeries_ShrinksAxisRange()
    {
        var small = new LineSeries { ItemsSource = new[] { new DataPoint(0, 0), new DataPoint(1, 4) } };
        var large = new LineSeries { ItemsSource = new[] { new DataPoint(0, 0), new DataPoint(1, 300) } };
        var plot = new Plot();
        plot.Axes.Add(new LinearAxis { Position = CoreAxes.AxisPosition.Bottom });
        plot.Axes.Add(new LinearAxis { Position = CoreAxes.AxisPosition.Left });
        plot.Series.Add(small);
        plot.Series.Add(large);
        var window = Ui.Show(plot);
        Assert.True(plot.ActualModel.Axes[1].ActualMaximum >= 300);

        large.IsVisible = false;
        Ui.Flush();

        Assert.True(plot.ActualModel.Axes[1].ActualMaximum < 10);
        window.Close();
    }

    [AvaloniaFact]
    public void PlotView_SetModelNull_ClearsCanvas()
    {
        var model = new PlotModel();
        model.Series.Add(new CoreSeries.FunctionSeries(Math.Sin, 0, 10, 0.1));
        var view = new PlotView { Model = model };
        var window = Ui.Show(view);
        var canvas = Ui.PlotCanvas(view);
        Assert.NotEmpty(canvas.Children);

        view.Model = null;
        Ui.Flush();

        // without a model the old picture must not stay on the screen
        Assert.Empty(canvas.Children);
        window.Close();
    }

    [AvaloniaFact]
    public void Plot_TitleHorizontalAlignment_ReachesModel()
    {
        var plot = new Plot { Title = "Title", TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinView };
        var window = Ui.Show(plot);

        Assert.Equal(TitleHorizontalAlignment.CenteredWithinView, plot.ActualModel.TitleHorizontalAlignment);

        plot.TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea;
        Ui.Flush();

        Assert.Equal(TitleHorizontalAlignment.CenteredWithinPlotArea, plot.ActualModel.TitleHorizontalAlignment);
        window.Close();
    }

    [AvaloniaFact]
    public void Plot_PlotAreaBackgroundFromImmutableBrush_ReachesModel()
    {
        // Brushes.* give immutable brushes: a property of type Brush did not accept them, so the test would not even compile
        var plot = new Plot { PlotAreaBackground = Brushes.LightGray };
        var window = Ui.Show(plot);

        Assert.Equal(OxyColors.LightGray, plot.ActualModel.PlotAreaBackground);
        window.Close();
    }

    [AvaloniaFact]
    public void Plot_InvalidateFlagChanged_SynchronizesWrappers()
    {
        var polygon = new PolygonAnnotation();
        var plot = new Plot();
        plot.Annotations.Add(polygon);
        var window = Ui.Show(plot);

        // the list of points is changed in place, without notifications: the model is updated only by an explicit invalidation
        polygon.Points.Add(new DataPoint(0, 0));
        polygon.Points.Add(new DataPoint(1, 0));
        polygon.Points.Add(new DataPoint(1, 1));
        plot.InvalidateFlag++;
        Ui.Flush();

        var annotation = Assert.IsType<CoreAnnotations.PolygonAnnotation>(Assert.Single(plot.ActualModel.Annotations));
        Assert.Equal(3, annotation.Points.Count);
        window.Close();
    }

    [AvaloniaFact]
    public void Plot_ClearSeries_DetachesSeriesFromTrees()
    {
        var plot = new Plot();
        var window = Ui.Show(plot);
        var series = new LineSeries();
        plot.Series.Add(series);
        Ui.Flush();

        plot.Series.Clear();
        Ui.Flush();

        Assert.Null(series.Parent);
        Assert.DoesNotContain(series, plot.GetLogicalChildren());
        Assert.DoesNotContain(series, plot.GetVisualChildren());
        Assert.Empty(plot.ActualModel.Series);
        window.Close();
    }

    [AvaloniaFact]
    public void Plot_ClearAndAddSameSeries_DoesNotThrow()
    {
        // Clear() used to leave the series in the visual tree, and adding it again failed.
        // The window is shown before adding: applying the template clears the visual children itself and would hide the error
        var plot = new Plot();
        var window = Ui.Show(plot);
        var series = new LineSeries();
        plot.Series.Add(series);
        Ui.Flush();

        plot.Series.Clear();
        plot.Series.Add(series);
        Ui.Flush();

        Assert.Same(plot, series.Parent);
        Assert.Single(plot.ActualModel.Series);
        window.Close();
    }

    [AvaloniaFact]
    public void Plot_MoveSeries_KeepsParentAndOrder()
    {
        var first = new LineSeries { Title = "a" };
        var second = new LineSeries { Title = "b" };
        var plot = new Plot();
        plot.Series.Add(first);
        plot.Series.Add(second);
        var window = Ui.Show(plot);

        ((ObservableCollection<Series>)plot.Series).Move(0, 1);
        Ui.Flush();

        Assert.Same(plot, first.Parent);
        Assert.Single(plot.GetLogicalChildren(), c => ReferenceEquals(c, first));
        Assert.Equal("b", plot.ActualModel.Series[0].Title);
        window.Close();
    }

    [AvaloniaFact]
    public void Plot_ReplaceSeriesWithSameInstance_KeepsParent()
    {
        var series = new LineSeries();
        var plot = new Plot();
        plot.Series.Add(series);
        var window = Ui.Show(plot);

        plot.Series[0] = plot.Series[0];
        Ui.Flush();

        Assert.Same(plot, series.Parent);
        Assert.Single(plot.GetLogicalChildren(), c => ReferenceEquals(c, series));
        Assert.Single(plot.ActualModel.Series);
        window.Close();
    }

    [AvaloniaFact]
    public void PlotViewSubclass_Shown_GetsTemplateAndRenders()
    {
        var model = new PlotModel();
        model.Series.Add(new CoreSeries.FunctionSeries(Math.Sin, 0, 10, 0.1));
        var view = new DerivedPlotView { Model = model };

        var window = Ui.Show(view);

        Assert.NotNull(view.Template);
        Assert.True(model.PlotArea.Width > 0);
        window.Close();
    }

    [AvaloniaFact]
    public void PlotSubclass_Shown_GetsTemplateAndRenders()
    {
        var plot = new DerivedPlot();
        plot.Series.Add(new LineSeries { ItemsSource = new[] { new DataPoint(0, 0), new DataPoint(1, 1) } });

        var window = Ui.Show(plot);

        Assert.NotNull(plot.Template);
        Assert.True(plot.ActualModel.PlotArea.Width > 0);
        window.Close();
    }

    [AvaloniaFact]
    public void PlotView_ShowZoomRectangle_DrawsVisibleBorder()
    {
        var view = new PlotView { Model = Ui.ModelWithAxes() };
        var window = Ui.Show(view);

        view.ShowZoomRectangle(new OxyRect(10, 10, 100, 50));
        Ui.Flush();

        // the stroke thickness of a Rectangle in Avalonia is 0 by default, and the border is not visible
        var rectangle = Assert.Single(view.GetVisualDescendants().OfType<Rectangle>());
        Assert.True(rectangle.StrokeThickness > 0);
        window.Close();
    }

    [AvaloniaFact]
    public void PlotView_InvalidatePlotFromBackgroundThread_UpdatesModel()
    {
        var series = new CoreSeries.LineSeries();
        var model = new PlotModel();
        model.Series.Add(series);
        var view = new PlotView { Model = model };
        var window = Ui.Show(view);

        series.Points.Add(new DataPoint(0, 0));
        series.Points.Add(new DataPoint(1, 50));
        var thread = new Thread(() => view.InvalidatePlot(true));
        thread.Start();
        thread.Join();
        Ui.Flush();

        // a call from a foreign thread goes to the dispatcher of the control and updates the data there
        Assert.Equal(50, series.MaxY);
        window.Close();
    }

    [AvaloniaFact]
    public void Legend_DefaultFontSizes_FollowPlotDefaultFontSize()
    {
        var legend = new Legend().CreateModel();

        // NaN in the core means "same as the DefaultFontSize of the plot", the number 12 broke that
        Assert.True(double.IsNaN(legend.LegendFontSize));
        Assert.True(double.IsNaN(legend.LegendTitleFontSize));
    }

    /// <summary>
    /// A descendant of PlotView: styles with an exact type selector did not find it.
    /// </summary>
    private sealed class DerivedPlotView : PlotView
    {
    }

    /// <summary>
    /// A descendant of Plot: styles with an exact type selector did not find it.
    /// </summary>
    private sealed class DerivedPlot : Plot
    {
    }
}
