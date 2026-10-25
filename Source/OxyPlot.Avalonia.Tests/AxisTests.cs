using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using OxyPlot;
using OxyPlot.Avalonia;
using CoreAxes = OxyPlot.Axes;

namespace OxyPlot.Avalonia.Tests;

/// <summary>
/// Tests of the axis controls: default values, value validation and copying to the axes of the core.
/// </summary>
public class AxisTests
{
    [AvaloniaFact]
    public void AngleAxis_Default_PositionAllAsInCore()
    {
        var axis = new AngleAxis();

        // with None the axis does not reserve margins and the angle labels are clipped
        Assert.Equal(new CoreAxes.AngleAxis().Position, axis.CreateModel().Position);
        Assert.Equal(CoreAxes.AxisPosition.All, axis.Position);
    }

    [AvaloniaFact]
    public void AngleAxis_Default_SolidGridlines()
    {
        var axis = new AngleAxis();

        Assert.Equal(LineStyle.Solid, axis.MajorGridlineStyle);
        Assert.Equal(LineStyle.Solid, axis.MinorGridlineStyle);
    }

    [AvaloniaFact]
    public void AngleAxis_StyleSetter_OverridesGridlineStyle()
    {
        var angle = new AngleAxis();
        var plot = new Plot { PlotType = PlotType.Polar };
        plot.Styles.Add(new Style(x => x.OfType<AngleAxis>())
        {
            Setters = { new Setter(Axis.MajorGridlineStyleProperty, LineStyle.Dot) },
        });
        plot.Axes.Add(angle);
        plot.Axes.Add(new MagnitudeAxis());

        var window = Ui.Show(plot);

        // the value from the constructor was a local one and overrode the style
        Assert.Equal(LineStyle.Dot, angle.MajorGridlineStyle);
        window.Close();
    }

    [AvaloniaFact]
    public void LogarithmicAxis_Default_FilterMinValueZero()
    {
        Assert.Equal(0.0, new LogarithmicAxis().FilterMinValue);
    }

    [AvaloniaFact]
    public void LogarithmicAxis_StyleSetter_OverridesFilterMinValue()
    {
        var axis = new LogarithmicAxis { Position = CoreAxes.AxisPosition.Left };
        var plot = new Plot();
        plot.Styles.Add(new Style(x => x.OfType<LogarithmicAxis>())
        {
            Setters = { new Setter(Axis.FilterMinValueProperty, -5.0) },
        });
        plot.Axes.Add(axis);

        var window = Ui.Show(plot);

        Assert.Equal(-5.0, axis.FilterMinValue);
        window.Close();
    }

    [AvaloniaTheory]
    [InlineData(1.0)]
    [InlineData(0.0)]
    [InlineData(-2.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void LogarithmicAxis_InvalidBase_Throws(double value)
    {
        // with such a base the core cannot compute the ticks and rendering hangs
        Assert.Throws<ArgumentException>(() => new LogarithmicAxis { Base = value });
    }

    [AvaloniaFact]
    public void LogarithmicAxis_BaseTwo_ReachesModel()
    {
        var model = Assert.IsType<CoreAxes.LogarithmicAxis>(new LogarithmicAxis { Base = 2 }.CreateModel());

        Assert.Equal(2, model.Base);
    }

    [AvaloniaFact]
    public void Axis_MarginsAndExtraGridlines_ReachModel()
    {
        var axis = new LinearAxis { MinimumMargin = 3, MaximumMargin = 7, ExtraGridlines = new[] { 1.0, 2.0 } };

        var model = axis.CreateModel();

        // MaximumMargin used to receive the value of MinimumMargin
        Assert.Equal(3, model.MinimumMargin);
        Assert.Equal(7, model.MaximumMargin);
        Assert.Equal(new[] { 1.0, 2.0 }, model.ExtraGridlines);

        // the name of the Avalonia property matches the name of the CLR property, otherwise the attribute in markup is not found
        Assert.Same(Axis.ExtraGridlinesProperty, AvaloniaPropertyRegistry.Instance.FindRegistered(axis, nameof(Axis.ExtraGridlines)));
    }

    [AvaloniaFact]
    public void LinearColorAxis_TwoStops_InterpolatesRequestedPaletteSize()
    {
        var axis = new LinearColorAxis { PaletteSize = 5 };
        axis.GradientStops.Add(new GradientStop(Colors.Black, 0));
        axis.GradientStops.Add(new GradientStop(Colors.White, 1));

        var palette = ((CoreAxes.LinearColorAxis)axis.CreateModel()).Palette.Colors;

        Assert.Equal(5, palette.Count);
        Assert.Equal(OxyColors.Black, palette[0]);
        Assert.Equal(OxyColors.White, palette[4]);
    }

    [AvaloniaFact]
    public void LinearColorAxis_PaletteSizeOne_GivesSingleColor()
    {
        var axis = new LinearColorAxis { PaletteSize = 1 };
        axis.GradientStops.Add(new GradientStop(Colors.Black, 0));
        axis.GradientStops.Add(new GradientStop(Colors.White, 1));

        var palette = ((CoreAxes.LinearColorAxis)axis.CreateModel()).Palette.Colors;

        Assert.Single(palette);
    }

    [AvaloniaFact]
    public void LinearColorAxis_StopsAtSameOffset_KeepsPaletteSize()
    {
        var axis = new LinearColorAxis { PaletteSize = 4 };
        axis.GradientStops.Add(new GradientStop(Colors.Red, 0.5));
        axis.GradientStops.Add(new GradientStop(Colors.Blue, 0.5));

        var palette = ((CoreAxes.LinearColorAxis)axis.CreateModel()).Palette.Colors;

        Assert.Equal(4, palette.Count);
    }

    [AvaloniaFact]
    public void LinearColorAxis_OneStop_GivesPaletteOfThatColor()
    {
        var axis = new LinearColorAxis { PaletteSize = 3 };
        axis.GradientStops.Add(new GradientStop(Colors.Red, 0.2));

        var palette = ((CoreAxes.LinearColorAxis)axis.CreateModel()).Palette.Colors;

        Assert.Equal(3, palette.Count);
        Assert.All(palette, c => Assert.Equal(OxyColors.Red, c));
    }

    [AvaloniaFact]
    public void LinearColorAxis_NoStops_KeepsCorePalette()
    {
        var palette = ((CoreAxes.LinearColorAxis)new LinearColorAxis().CreateModel()).Palette.Colors;

        Assert.Equal(new CoreAxes.LinearColorAxis().Palette.Colors, palette);
    }

    [AvaloniaFact]
    public void LinearColorAxis_PaletteSizeZero_Throws()
    {
        Assert.Throws<ArgumentException>(() => new LinearColorAxis { PaletteSize = 0 });
    }

    [AvaloniaFact]
    public void CategoryAxis_TwoInstances_HaveSeparateLabelLists()
    {
        var first = new CategoryAxis();
        var second = new CategoryAxis();

        first.Labels.Add("a");

        Assert.NotSame(first.Labels, second.Labels);
        Assert.Empty(second.Labels);
    }
}
