using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using OxyPlot;
using OxyPlot.Avalonia;

namespace OxyPlot.Avalonia.Tests;

/// <summary>
/// Input tests: the wheel, the right button with a context menu, the loss of the pointer capture.
/// </summary>
public class InputTests
{
    private static readonly Point Center = new(200, 150);

    [AvaloniaTheory]
    [InlineData(0.25)]
    [InlineData(-0.25)]
    [InlineData(1.0)]
    public void PlotView_WheelDelta_ZoomsAxis(double delta)
    {
        // a touchpad gives a fractional step: rounding before the multiplication turned it into zero
        var model = Ui.ModelWithAxes();
        var view = new PlotView { Model = model };
        var window = Ui.Show(view);
        var range = model.Axes[0].ActualMaximum - model.Axes[0].ActualMinimum;

        window.MouseWheel(Center, new Vector(0, delta));
        Ui.Flush();

        var newRange = model.Axes[0].ActualMaximum - model.Axes[0].ActualMinimum;
        if (delta > 0)
        {
            Assert.True(newRange < range);
        }
        else
        {
            Assert.True(newRange > range);
        }

        window.Close();
    }

    [AvaloniaFact]
    public void PlotView_RightClickWithContextMenu_CompletesControllerAndReleasesCapture()
    {
        var controller = new CountingController();
        var view = new PlotView { Model = Ui.ModelWithAxes(), Controller = controller, ContextMenu = new ContextMenu() };
        var pointer = TrackPointer(view);
        var window = Ui.Show(view);

        window.MouseDown(Center, MouseButton.Right);
        window.MouseUp(Center, MouseButton.Right);
        Ui.Flush();

        // the context menu marks the release as handled, but the controller still receives it
        Assert.Equal(1, controller.MouseUps);
        Assert.Null(pointer().Captured);
        window.Close();
    }

    [AvaloniaFact]
    public void PlotView_RightDragThenRelease_SuppressesContextMenu()
    {
        var view = new PlotView { Model = Ui.ModelWithAxes(), ContextMenu = new ContextMenu() };
        bool? handled = null;
        view.AddHandler(Control.ContextRequestedEvent, (_, e) => handled = e.Handled, RoutingStrategies.Bubble, handledEventsToo: true);
        var window = Ui.Show(view);

        window.MouseDown(Center, MouseButton.Right);
        window.MouseMove(Center + new Point(50, 20), RawInputModifiers.RightMouseButton);
        window.MouseUp(Center + new Point(50, 20), MouseButton.Right);
        Ui.Flush();

        // the menu is not shown after panning with the right button
        Assert.True(handled);
        Assert.False(view.ContextMenu.IsOpen);
        window.Close();
    }

    [AvaloniaFact]
    public void PlotView_RightClickWithoutDrag_AllowsContextMenu()
    {
        // without a menu of its own nobody else handles the event, so the decision of the plot itself is visible
        var view = new PlotView { Model = Ui.ModelWithAxes() };
        bool? handled = null;
        view.AddHandler(Control.ContextRequestedEvent, (_, e) => handled = e.Handled, RoutingStrategies.Bubble, handledEventsToo: true);
        var window = Ui.Show(view);

        window.MouseDown(Center, MouseButton.Right);
        window.MouseUp(Center, MouseButton.Right);
        Ui.Flush();

        Assert.False(handled);
        window.Close();
    }

    [AvaloniaFact]
    public void PlotView_CaptureLostWhilePanning_StopsPanning()
    {
        var model = Ui.ModelWithAxes();
        var view = new PlotView { Model = model };
        var pointer = TrackPointer(view);
        var window = Ui.Show(view);

        window.MouseDown(Center, MouseButton.Right);
        window.MouseMove(Center + new Point(50, 0), RawInputModifiers.RightMouseButton);
        Ui.Flush();

        // the system released the capture and the control never received the button release
        pointer().Capture(null);
        var minimum = model.Axes[0].ActualMinimum;
        window.MouseMove(Center + new Point(100, 0));
        Ui.Flush();

        Assert.Equal(minimum, model.Axes[0].ActualMinimum);
        window.Close();
    }

    [AvaloniaFact]
    public void PlotView_TouchCaptureLost_NextTouchStartsGesture()
    {
        var controller = new CountingController();
        var view = new PlotView { Model = Ui.ModelWithAxes(), Controller = controller };
        var pointer = TrackPointer(view);
        var window = Ui.Show(view);

        window.TouchBegin(Center);
        Ui.Flush();
        pointer().Capture(null);
        Ui.Flush();

        // the touch is completed and not left hanging in the list of active ones
        Assert.Equal(1, controller.TouchCompletions);

        window.TouchBegin(Center + new Point(-50, -50));
        Ui.Flush();

        Assert.Equal(2, controller.TouchStarts);
        window.Close();
    }

    /// <summary>
    /// Remembers the pointer of the last press on the control.
    /// </summary>
    private static Func<IPointer> TrackPointer(Control control)
    {
        IPointer? pointer = null;
        control.AddHandler(InputElement.PointerPressedEvent, (_, e) => pointer = e.Pointer, RoutingStrategies.Bubble, handledEventsToo: true);
        return () => pointer ?? throw new InvalidOperationException("The control was not pressed.");
    }

    /// <summary>
    /// A controller that counts the calls that matter for the tests.
    /// </summary>
    private sealed class CountingController : PlotController
    {
        public int MouseUps { get; private set; }

        public int TouchStarts { get; private set; }

        public int TouchCompletions { get; private set; }

        public override bool HandleMouseUp(IView view, OxyMouseEventArgs args)
        {
            MouseUps++;
            return base.HandleMouseUp(view, args);
        }

        public override bool HandleTouchStarted(IView view, OxyTouchEventArgs args)
        {
            TouchStarts++;
            return base.HandleTouchStarted(view, args);
        }

        public override bool HandleTouchCompleted(IView view, OxyTouchEventArgs args)
        {
            TouchCompletions++;
            return base.HandleTouchCompleted(view, args);
        }
    }
}
