using System.Reflection;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using OxyPlot;
using OxyPlot.Avalonia;
using CoreAxes = OxyPlot.Axes;
using CoreSeries = OxyPlot.Series;

namespace OxyPlot.Avalonia.Tests;

/// <summary>
/// Tests of the series controls: copying the properties to the series of the core and the reaction of the plot to their changes.
/// </summary>
public class SeriesTests
{
    private static readonly DataPoint[] Points = { new(0, 0), new(1, 1), new(2, 4) };

    [AvaloniaFact]
    public void TwoColorLineSeries_LineStyle2_ReachesModel()
    {
        var series = new TwoColorLineSeries { LineStyle2 = LineStyle.Dash };

        var model = Assert.IsType<CoreSeries.TwoColorLineSeries>(series.CreateModel());

        Assert.Equal(LineStyle.Dash, model.LineStyle2);
    }

    [AvaloniaFact]
    public void ThreeColorLineSeries_LineStylesLoAndHi_ReachModel()
    {
        var series = new ThreeColorLineSeries { LineStyleLo = LineStyle.Dash, LineStyleHi = LineStyle.Dot };

        var model = Assert.IsType<CoreSeries.ThreeColorLineSeries>(series.CreateModel());

        Assert.Equal(LineStyle.Dash, model.LineStyleLo);
        Assert.Equal(LineStyle.Dot, model.LineStyleHi);
    }

    [AvaloniaFact]
    public void LineSeries_ChangeLabelAndInterpolationAfterShow_ReachesModel()
    {
        var series = new LineSeries { ItemsSource = Points };
        var plot = new Plot();
        plot.Series.Add(series);
        var window = Ui.Show(plot);

        series.LabelFormatString = "{1}";
        series.LabelMargin = 10;
        series.LineLegendPosition = CoreSeries.LineLegendPosition.End;
        series.InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline;
        Ui.Flush();

        var model = (CoreSeries.LineSeries)series.InternalSeries;
        Assert.Equal("{1}", model.LabelFormatString);
        Assert.Equal(10, model.LabelMargin);
        Assert.Equal(CoreSeries.LineLegendPosition.End, model.LineLegendPosition);
        Assert.Same(InterpolationAlgorithms.CanonicalSpline, model.InterpolationAlgorithm);
        window.Close();
    }

    [AvaloniaFact]
    public void DataPointSeries_ChangeMappingAfterShow_ReloadsData()
    {
        var samples = new[] { new Sample(0, 1), new Sample(1, 2) };
        var series = new LineSeries { ItemsSource = samples, Mapping = o => new DataPoint(((Sample)o).X, ((Sample)o).Y) };
        var plot = new Plot();
        plot.Series.Add(series);
        var window = Ui.Show(plot);
        Assert.Equal(2, ((CoreSeries.XYAxisSeries)series.InternalSeries).MaxY);

        series.Mapping = o => new DataPoint(((Sample)o).X, ((Sample)o).Y * 10);
        Ui.Flush();

        Assert.Equal(20, ((CoreSeries.XYAxisSeries)series.InternalSeries).MaxY);
        window.Close();
    }

    [AvaloniaFact]
    public void TwoColorAreaSeries_ChangeLimitAfterShow_SplitsPointsByNewLimit()
    {
        var series = new TwoColorAreaSeries
        {
            ItemsSource = new[] { new DataPoint(0, -1), new DataPoint(1, 1), new DataPoint(2, -1) },
            Limit = 0,
        };
        var plot = new Plot();
        plot.Series.Add(series);
        var window = Ui.Show(plot);

        series.Limit = 0.5;
        Ui.Flush();

        // the core inserts the points of intersection with the limit while splitting in UpdateData
        var above = (List<DataPoint>)typeof(CoreSeries.TwoColorAreaSeries)
            .GetField("abovePoints", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(series.InternalSeries)!;
        Assert.Contains(above, p => Math.Abs(p.Y - 0.5) < 1e-9);
        window.Close();
    }

    [AvaloniaFact]
    public void BarSeries_SetIsStackedAfterShow_RangesValueAxisBySum()
    {
        var first = new BarSeries { ItemsSource = new[] { new CoreSeries.BarItem(3), new CoreSeries.BarItem(5) } };
        var second = new BarSeries { ItemsSource = new[] { new CoreSeries.BarItem(4), new CoreSeries.BarItem(6) } };
        var plot = new Plot();
        plot.Axes.Add(new CategoryAxis { Position = CoreAxes.AxisPosition.Left });
        plot.Axes.Add(new LinearAxis { Position = CoreAxes.AxisPosition.Bottom });
        plot.Series.Add(first);
        plot.Series.Add(second);
        var window = Ui.Show(plot);
        Assert.True(plot.ActualModel.Axes[1].ActualMaximum < 11);

        first.IsStacked = true;
        second.IsStacked = true;
        Ui.Flush();

        // the stack sum 5 + 6 is computed in UpdateMaxMin, that is, only when the data is updated
        Assert.True(plot.ActualModel.Axes[1].ActualMaximum >= 11);
        window.Close();
    }

    [AvaloniaFact]
    public void PieSeries_ChangeExplodedDistanceAfterShow_ReachesModel()
    {
        var series = new PieSeries();
        var plot = new Plot();
        plot.Series.Add(series);
        var window = Ui.Show(plot);

        series.ExplodedDistance = 0.3;
        Ui.Flush();

        Assert.Equal(0.3, ((CoreSeries.PieSeries)series.InternalSeries).ExplodedDistance);
        window.Close();
    }

    [AvaloniaFact]
    public void HeatMapSeries_ChangeX1AfterShow_RangesXAxis()
    {
        var series = new HeatMapSeries
        {
            Data = new double[,] { { 1, 2 }, { 3, 4 } },
            X0 = 0,
            X1 = 1,
            Y0 = 0,
            Y1 = 1,
            ColorAxisKey = "c",
        };
        var plot = new Plot();
        plot.Axes.Add(new LinearAxis { Position = CoreAxes.AxisPosition.Bottom });
        plot.Axes.Add(new LinearAxis { Position = CoreAxes.AxisPosition.Left });
        plot.Axes.Add(new LinearColorAxis { Position = CoreAxes.AxisPosition.Right, Key = "c" });
        plot.Series.Add(series);
        var window = Ui.Show(plot);

        series.X1 = 10;
        Ui.Flush();

        Assert.True(((CoreSeries.XYAxisSeries)series.InternalSeries).MaxX >= 10);
        Assert.True(plot.ActualModel.Axes[0].ActualMaximum >= 10);
        window.Close();
    }

    [AvaloniaFact]
    public void LinearBarSeries_Defaults_MatchCore()
    {
        var model = Assert.IsType<CoreSeries.LinearBarSeries>(new LinearBarSeries().CreateModel());
        var core = new CoreSeries.LinearBarSeries();

        Assert.Equal(core.BarWidth, model.BarWidth);
        Assert.Equal(core.StrokeThickness, model.StrokeThickness);
    }

    [AvaloniaFact]
    public void ScatterSeries_ChangeColorAxisKeyAfterShow_ReachesModel()
    {
        var series = new ScatterSeries();
        var plot = new Plot();
        plot.Series.Add(series);
        var window = Ui.Show(plot);

        series.ColorAxisKey = "c";
        Ui.Flush();

        Assert.Equal("c", ((CoreSeries.ScatterSeries)series.InternalSeries).ColorAxisKey);
        window.Close();
    }

    [AvaloniaFact]
    public void Series_InheritedForegroundAndFont_DoNotReachModel()
    {
        // in a dark theme the window passes a white Foreground down to the series, while the plot background is white
        var series = new LineSeries { ItemsSource = Points, LabelFormatString = "{1}" };
        var plot = new Plot();
        plot.Series.Add(series);
        var window = Ui.Show(plot, ThemeVariant.Dark);

        Assert.Equal(OxyColors.Automatic, series.InternalSeries.TextColor);
        Assert.Null(series.InternalSeries.Font);

        series.Foreground = Brushes.Red;
        Ui.Flush();

        Assert.Equal(OxyColors.Red, series.InternalSeries.TextColor);
        window.Close();
    }

    [AvaloniaFact]
    public void BarSeries_ForeignItemsInItemsSource_AreSkipped()
    {
        var series = new BarSeries { ItemsSource = new object[] { new CoreSeries.BarItem(1), "foreign object" } };

        var model = Assert.IsType<CoreSeries.BarSeries>(series.CreateModel());

        Assert.Single(model.Items);
    }

    [AvaloniaFact]
    public void BoxPlotSeries_ForeignItemsInItemsSource_AreSkipped()
    {
        var series = new BoxPlotSeries { ItemsSource = new object[] { new CoreSeries.BoxPlotItem(0, 1, 2, 3, 4, 5), "foreign object" } };

        var model = Assert.IsType<CoreSeries.BoxPlotSeries>(series.CreateModel());

        Assert.Single(model.Items);
    }

    /// <summary>
    /// A data item that the series converts to a point through Mapping.
    /// </summary>
    private sealed record Sample(double X, double Y);
}
