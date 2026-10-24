// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlotBase.Events.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Represents a control that displays a <see cref="PlotModel" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Avalonia.Controls;

namespace OxyPlot.Avalonia
{
    using global::Avalonia.Input;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a control that displays a <see cref="PlotModel" />.
    /// </summary>
    public partial class PlotBase
    {
        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.KeyDown" /> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Handled)
            {
                return;
            }

            var args = new OxyKeyEventArgs { ModifierKeys = e.KeyModifiers.ToModifierKeys(), Key = e.Key.Convert() };
            e.Handled = ActualController.HandleKeyDown(this, args);
        }

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.MouseWheel" /> event occurs to provide handling for the event in a derived class without attaching a delegate.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Input.MouseWheelEventArgs" /> that contains the event data.</param>
        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            if (e.Handled || !IsMouseWheelEnabled)
            {
                return;
            }

            e.Handled = ActualController.HandleMouseWheel(this, e.ToMouseWheelEventArgs(this));
        }

        /// <summary>
        /// Gets the dictionary of locations of touch pointers.
        /// </summary>
        private SortedDictionary<int, ScreenPoint> TouchPositions { get; } = new SortedDictionary<int, ScreenPoint>();

        /// <summary>
        /// The last known position of the pointer.
        /// </summary>
        private ScreenPoint lastPointerPosition;

        /// <summary>
        /// Indicates that the capture is released by <see cref="OnPointerReleased"/> itself, not by the system.
        /// </summary>
        private bool isReleasingPointer;

        /// <summary>
        /// Invoked when an unhandled MouseDown attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. This event data reports details about the mouse button that was pressed and the handled state.</param>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (e.Handled)
            {
                return;
            }

            Focus();
            e.Pointer.Capture(this);

            // The press point is remembered for any pointer: it decides whether the context menu is shown.
            mouseDownPoint = e.GetPosition(this).ToScreenPoint();
            mouseDownPointerType = e.Pointer.Type;
            hasMouseDownPoint = true;
            lastPointerPosition = mouseDownPoint;

            if (e.Pointer.Type == PointerType.Touch)
            {
                var position = e.GetPosition(this).ToScreenPoint();

                var touchEventArgs = new OxyTouchEventArgs()
                {
                    ModifierKeys = e.KeyModifiers.ToModifierKeys(),
                    Position = position,
                    DeltaTranslation = new ScreenVector(0, 0),
                    DeltaScale = new ScreenVector(1, 1),
                };

                TouchPositions[e.Pointer.Id] = position;

                if (TouchPositions.Count == 1)
                {
                    e.Handled = ActualController.HandleTouchStarted(this, touchEventArgs);
                }
            }
            else
            {
                e.Handled = ActualController.HandleMouseDown(this, e.ToMouseDownEventArgs(this));
            }
        }

        /// <summary>
        /// Invoked when an unhandled MouseMove attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (e.Handled)
            {
                return;
            }

            lastPointerPosition = e.GetPosition(this).ToScreenPoint();

            if (e.Pointer.Type == PointerType.Touch)
            {
                var point = e.GetPosition(this).ToScreenPoint();
                var oldTouchPoints = TouchPositions.Values.ToArray();
                TouchPositions[e.Pointer.Id] = point;
                var newTouchPoints = TouchPositions.Values.ToArray();

                var touchEventArgs = new OxyTouchEventArgs(newTouchPoints, oldTouchPoints);

                e.Handled = ActualController.HandleTouchDelta(this, touchEventArgs);
            }
            else
            {
                e.Handled = ActualController.HandleMouseMove(this, e.ToMouseEventArgs(this));
            }
        }

        /// <summary>
        /// Invoked when an unhandled MouseUp routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the mouse button was released.</param>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            // The state is remembered before the base method: it raises ContextRequested
            // and copies its Handled here. Otherwise, when a context menu is set,
            // the controller would not receive the release and the manipulator would stay active.
            var handledBefore = e.Handled;
            base.OnPointerReleased(e);

            // The capture is always released, even if the event has already been marked as handled.
            // Otherwise the pointer stays captured after a right click and panning gets stuck.
            // Releasing the capture synchronously raises PointerCaptureLost: the flag distinguishes it from a loss of
            // capture by the system, otherwise the manipulator would be completed twice.
            isReleasingPointer = true;
            try
            {
                e.Pointer.Capture(null);
            }
            finally
            {
                isReleasingPointer = false;
            }

            if (handledBefore)
            {
                return;
            }

            if (e.Pointer.Type == PointerType.Touch)
            {
                var position = e.GetPosition(this).ToScreenPoint();

                var touchEventArgs = new OxyTouchEventArgs()
                {
                    ModifierKeys = e.KeyModifiers.ToModifierKeys(),
                    Position = position,
                    DeltaTranslation = new ScreenVector(0, 0),
                    DeltaScale = new ScreenVector(1, 1),
                };

                TouchPositions.Remove(e.Pointer.Id);

                if (TouchPositions.Count == 0)
                {
                    e.Handled = ActualController.HandleTouchCompleted(this, touchEventArgs);
                }
            }
            else
            {
                e.Handled = ActualController.HandleMouseUp(this, e.ToMouseReleasedEventArgs(this));
            }
        }

        /// <summary>
        /// Completes the action started by a press if the pointer capture is lost without a release:
        /// the window lost activation, the system cancelled the touch, etc. Otherwise the manipulator of the controller
        /// stays active and the plot is panned by plain mouse movement, and the touch stays forever
        /// in <see cref="TouchPositions"/>, so the following gestures do not start.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            if (isReleasingPointer)
            {
                return;
            }

            if (e.Pointer.Type == PointerType.Touch)
            {
                if (TouchPositions.TryGetValue(e.Pointer.Id, out var position))
                {
                    TouchPositions.Remove(e.Pointer.Id);
                    if (TouchPositions.Count == 0)
                    {
                        ActualController.HandleTouchCompleted(this, new OxyTouchEventArgs
                        {
                            Position = position,
                            DeltaTranslation = new ScreenVector(0, 0),
                            DeltaScale = new ScreenVector(1, 1),
                        });
                    }
                }
            }
            else
            {
                ActualController.HandleMouseUp(this, new OxyMouseEventArgs { Position = lastPointerPosition });
            }
        }

        /// <summary>
        /// Prepares the context menu for display. The menu is opened by Avalonia itself on the
        /// <see cref="InputElement.ContextRequestedEvent"/> event, so it does not need to be opened manually.
        /// If the pointer was moved after the button was pressed (a zoom rectangle was drawn), the menu is not shown.
        /// </summary>
        /// <param name="sender">The event source.</param>
        /// <param name="e">The event data.</param>
        private void OnPlotContextRequested(object sender, ContextRequestedEventArgs e)
        {
            // The menu is suppressed only for the mouse: for touch and pen it is opened by a long press,
            // and the press point is irrelevant to that.
            if (hasMouseDownPoint && mouseDownPointerType == PointerType.Mouse
                && e.TryGetPosition(this, out var position)
                && position.ToScreenPoint().DistanceTo(mouseDownPoint) > 1e-8)
            {
                // A handled event suppresses opening of the menu.
                e.Handled = true;
                return;
            }

            if (ContextMenu != null)
            {
                ContextMenu.DataContext = DataContext;
            }
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> attached event is raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            if (e.Handled)
            {
                return;
            }

            e.Handled = ActualController.HandleMouseEnter(this, e.ToMouseEventArgs(this));
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseLeave" /> attached event is raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            if (e.Handled)
            {
                return;
            }

            e.Handled = ActualController.HandleMouseLeave(this, e.ToMouseEventArgs(this));
        }
    }
}