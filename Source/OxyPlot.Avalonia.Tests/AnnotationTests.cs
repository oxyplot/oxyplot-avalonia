using Avalonia.Headless.XUnit;
using Avalonia.Media;
using OxyPlot;
using OxyPlot.Avalonia;
using AvaloniaHorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using AvaloniaVerticalAlignment = Avalonia.Layout.VerticalAlignment;
using CoreAnnotations = OxyPlot.Annotations;
using CoreAxes = OxyPlot.Axes;
using ShapePath = Avalonia.Controls.Shapes.Path;

namespace OxyPlot.Avalonia.Tests;

/// <summary>
/// Tests of the annotation controls: default values, copying to the core and rendering without failures.
/// </summary>
public class AnnotationTests
{
    [AvaloniaFact]
    public void RectangleAnnotation_HorizontalBand_DrawsAcrossPlotArea()
    {
        var plot = PlotWithAxes();
        plot.Annotations.Add(new RectangleAnnotation { MinimumY = 4, MaximumY = 6 });

        // Show used to throw InvalidOperationException from Measure: the rectangle went to infinity
        var window = Ui.Show(plot);

        var plotArea = plot.ActualModel.PlotArea;
        var band = Assert.Single(
            Ui.PlotCanvas(plot).Children.OfType<ShapePath>(),
            p => p.Fill != null && p.Data is RectangleGeometry);
        var rect = ((RectangleGeometry)band.Data!).Rect;
        Assert.Equal(plotArea.Width, rect.Width, 3);
        Assert.True(rect.Height > 0 && rect.Height < plotArea.Height);
        window.Close();
    }

    [AvaloniaFact]
    public void RectangleAnnotation_Defaults_AreInfiniteAsInCore()
    {
        var annotation = new RectangleAnnotation();

        Assert.Equal(double.NegativeInfinity, annotation.MinimumX);
        Assert.Equal(double.PositiveInfinity, annotation.MaximumX);
        Assert.Equal(double.NegativeInfinity, annotation.MinimumY);
        Assert.Equal(double.PositiveInfinity, annotation.MaximumY);
    }

    [AvaloniaFact]
    public void RectangleAnnotation_LegacyMinMaxValues_BecomeInfiniteInModel()
    {
        // the previous values double.MinValue/MaxValue still mean "no bound"
        var annotation = new RectangleAnnotation
        {
            MinimumX = double.MinValue,
            MaximumX = double.MaxValue,
            MinimumY = 1,
            MaximumY = 2,
        };

        var model = Assert.IsType<CoreAnnotations.RectangleAnnotation>(annotation.CreateModel());

        Assert.Equal(double.NegativeInfinity, model.MinimumX);
        Assert.Equal(double.PositiveInfinity, model.MaximumX);
        Assert.Equal(1, model.MinimumY);
        Assert.Equal(2, model.MaximumY);
    }

    [AvaloniaFact]
    public void EllipseAnnotation_WithoutSize_DoesNotBreakLayout()
    {
        var plot = PlotWithAxes();
        plot.Annotations.Add(new EllipseAnnotation { X = 5, Y = 5 });

        var window = Ui.Show(plot);

        Assert.True(plot.ActualModel.PlotArea.Width > 0);
        window.Close();
    }

    [AvaloniaFact]
    public void TextAnnotation_WithoutPosition_DoesNotBreakLayout()
    {
        var plot = PlotWithAxes();
        plot.Annotations.Add(new TextAnnotation { Text = "label" });

        var window = Ui.Show(plot);

        Assert.True(plot.ActualModel.PlotArea.Width > 0);
        window.Close();
    }

    [AvaloniaFact]
    public void PlotView_CoreEllipseAnnotationWithoutSize_DoesNotBreakLayout()
    {
        // the core itself builds an ellipse with a NaN size, so the protection is needed in the render context
        var model = Ui.ModelWithAxes();
        model.Annotations.Add(new CoreAnnotations.EllipseAnnotation { X = 5, Y = 5 });
        var view = new PlotView { Model = model };

        var window = Ui.Show(view);

        Assert.True(model.PlotArea.Width > 0);
        window.Close();
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(-1)]
    public void FunctionAnnotation_NonPositiveResolution_Throws(int value)
    {
        // the core divides the range by Resolution and steps to the bound: the loop would never end
        Assert.Throws<ArgumentException>(() => new FunctionAnnotation { Resolution = value });
    }

    [AvaloniaFact]
    public void PolygonAnnotation_TwoInstances_HaveSeparatePointLists()
    {
        var first = new PolygonAnnotation();
        var second = new PolygonAnnotation();

        first.Points.Add(new DataPoint(1, 1));

        Assert.NotSame(first.Points, second.Points);
        Assert.Empty(second.Points);
    }

    [AvaloniaFact]
    public void PolylineAnnotation_TwoInstances_HaveSeparatePointLists()
    {
        var first = new PolylineAnnotation();
        var second = new PolylineAnnotation();

        first.Points.Add(new DataPoint(1, 1));

        Assert.NotSame(first.Points, second.Points);
        Assert.Empty(second.Points);
    }

    [AvaloniaFact]
    public void TextAnnotation_TextAlignment_ReachesModelInsteadOfLayoutAlignment()
    {
        // the layout alignment of the control must not replace the text alignment
        var annotation = new TextAnnotation
        {
            Text = "label",
            TextHorizontalAlignment = AvaloniaHorizontalAlignment.Left,
            TextVerticalAlignment = AvaloniaVerticalAlignment.Bottom,
            HorizontalAlignment = AvaloniaHorizontalAlignment.Right,
            VerticalAlignment = AvaloniaVerticalAlignment.Top,
        };

        var model = Assert.IsType<CoreAnnotations.TextAnnotation>(annotation.CreateModel());

        Assert.Equal(HorizontalAlignment.Left, model.TextHorizontalAlignment);
        Assert.Equal(VerticalAlignment.Bottom, model.TextVerticalAlignment);
    }

    /// <summary>
    /// A plot with linear axes 0..10 on which the annotations get screen coordinates.
    /// </summary>
    private static Plot PlotWithAxes()
    {
        var plot = new Plot();
        plot.Axes.Add(new LinearAxis { Position = CoreAxes.AxisPosition.Bottom, Minimum = 0, Maximum = 10 });
        plot.Axes.Add(new LinearAxis { Position = CoreAxes.AxisPosition.Left, Minimum = 0, Maximum = 10 });
        return plot;
    }
}
