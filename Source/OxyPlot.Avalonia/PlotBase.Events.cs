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
    using System;
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
                // store the mouse down point, check it when mouse button is released to determine if the context menu should be shown
                mouseDownPoint = e.GetPosition(this).ToScreenPoint();

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
            base.OnPointerReleased(e);
            if (e.Handled)
            {
                return;
            }

            e.Pointer.Capture(null);

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

                // Open the context menu
                var p = e.GetPosition(this).ToScreenPoint();
                var d = p.DistanceTo(mouseDownPoint);

                if (ContextMenu != null)
                {
                    if (Math.Abs(d) < 1e-8 && e.InitialPressMouseButton == MouseButton.Right)
                    {
                        ContextMenu.DataContext = DataContext;
                        ContextMenu.IsVisible = true;
                    }
                    else
                    {
                        ContextMenu.IsVisible = false;
                    }
                }
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