using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OxyPlot;
using OxyPlot.Avalonia;
using CoreAxes = OxyPlot.Axes;

namespace OxyPlot.Avalonia.Tests;

/// <summary>
/// Common actions of the tests: showing a control in a window of the headless application and running the deferred work.
/// </summary>
internal static class Ui
{
    /// <summary>
    /// Shows a control in a window and runs the deferred work of the dispatcher (the model update and rendering)
    /// </summary>
    public static Window Show(Control content, ThemeVariant? theme = null)
    {
        var window = new Window { Width = 400, Height = 300, Content = content };
        if (theme != null)
        {
            window.RequestedThemeVariant = theme;
        }

        window.Show();
        Flush();
        return window;
    }

    /// <summary>
    /// Runs the accumulated dispatcher work: the deferred model updates and the layout.
    /// </summary>
    public static void Flush()
    {
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>
    /// The canvas on which the control renders the model (the first canvas of the template, not the tracker layer).
    /// </summary>
    public static Canvas PlotCanvas(PlotBase plot)
    {
        return plot.GetVisualDescendants().OfType<Canvas>().First(c => c.Name != "Overlays");
    }

    /// <summary>
    /// A model with axes 0..10 for both coordinates: panning and zooming are tested on it.
    /// </summary>
    public static PlotModel ModelWithAxes()
    {
        var model = new PlotModel();
        model.Axes.Add(new CoreAxes.LinearAxis { Position = CoreAxes.AxisPosition.Bottom, Minimum = 0, Maximum = 10 });
        model.Axes.Add(new CoreAxes.LinearAxis { Position = CoreAxes.AxisPosition.Left, Minimum = 0, Maximum = 10 });
        return model;
    }
}
