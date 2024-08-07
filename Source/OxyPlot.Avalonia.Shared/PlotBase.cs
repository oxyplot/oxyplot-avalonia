// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlotBase.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Represents a control that displays a <see cref="PlotModel" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Avalonia.Reactive;

namespace OxyPlot.Avalonia
{
    using global::Avalonia;
    using global::Avalonia.Controls;
    using global::Avalonia.Controls.Primitives;
    using global::Avalonia.Input;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Represents a control that displays a <see cref="PlotModel" />.
    /// </summary>
    public abstract partial class PlotBase : TemplatedControl, IPlotView
    {
        private Control currentTracker;
        protected Panel panel;
        private Canvas overlays;
        private ContentControl zoomControl;
        private ScreenPoint mouseDownPoint;

        protected const string PartPanel = "PART_Panel";

        /// <summary>
        /// Invalidation flag (0: no update, 1: update, 2: update date).
        /// </summary>
        protected internal int isUpdateRequired;

        /// <summary>
        /// Invalidation flag (0: no update, 1: update visual elements).
        /// </summary>
        protected internal int isRenderRequired;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlotBase" /> class.
        /// </summary>
        protected PlotBase()
        {
            this.TrackerDefinitions = [];
            this.GetObservable(BoundsProperty).Subscribe(new AnonymousObserver<Rect>(this.OnSizeChanged));
        }

        /// <summary>
        /// Gets the coordinates of the client area of the view.
        /// </summary>
        public OxyRect ClientArea => new(0, 0, this.Bounds.Width, this.Bounds.Height);

        /// <summary>
        /// Gets the tracker definitions.
        /// </summary>
        /// <value>The tracker definitions.</value>
        public ObservableCollection<TrackerDefinition> TrackerDefinitions { get; }

        /// <summary>
        /// Hides the tracker.
        /// </summary>
        public void HideTracker()
        {
            if (this.currentTracker != null)
            {
                this.overlays.Children.Remove(this.currentTracker);
                this.currentTracker = null;
            }
        }

        /// <summary>
        /// Hides the zoom rectangle.
        /// </summary>
        public void HideZoomRectangle()
        {
            this.zoomControl.IsVisible = false;
        }

        /// <summary>
        /// Pans all axes.
        /// </summary>
        /// <param name="delta">The delta.</param>
        public void PanAllAxes(Vector delta)
        {
            this.ActualModel?.PanAllAxes(delta.X, delta.Y);
            this.InvalidatePlot(false);
        }

        /// <summary>
        /// Zooms all axes.
        /// </summary>
        /// <param name="factor">The zoom factor.</param>
        public void ZoomAllAxes(double factor)
        {
            this.ActualModel?.ZoomAllAxes(factor);
            this.InvalidatePlot(false);
        }

        /// <summary>
        /// Resets all axes.
        /// </summary>
        public void ResetAllAxes()
        {
            this.ActualModel?.ResetAllAxes();
            this.InvalidatePlot(false);
        }

        /// <summary>
        /// Invalidate the PlotView (not blocking the UI thread)
        /// </summary>
        /// <param name="updateData">The update Data.</param>
        public virtual void InvalidatePlot(bool updateData = true)
        {
            this.isUpdateRequired = updateData ? 2 : 1;
        }

        /// <summary>
        /// Sets the cursor type.
        /// </summary>
        /// <param name="cursorType">The cursor type.</param>
        public void SetCursorType(CursorType cursorType)
        {
            this.Cursor = cursorType switch
            {
                CursorType.Pan => this.PanCursor,
                CursorType.ZoomRectangle => this.ZoomRectangleCursor,
                CursorType.ZoomHorizontal => this.ZoomHorizontalCursor,
                CursorType.ZoomVertical => this.ZoomVerticalCursor,
                _ => Cursor.Default,
            };
        }

        /// <summary>
        /// Shows the tracker.
        /// </summary>
        /// <param name="trackerHitResult">The tracker data.</param>
        public void ShowTracker(TrackerHitResult trackerHitResult)
        {
            if (trackerHitResult == null)
            {
                this.HideTracker();
                return;
            }

            var trackerTemplate = this.DefaultTrackerTemplate;
            if (trackerHitResult.Series != null && !string.IsNullOrEmpty(trackerHitResult.Series.TrackerKey))
            {
                var match = this.TrackerDefinitions.FirstOrDefault(t => t.TrackerKey == trackerHitResult.Series.TrackerKey);
                if (match != null)
                {
                    trackerTemplate = match.TrackerTemplate;
                }
            }

            if (trackerTemplate == null)
            {
                this.HideTracker();
                return;
            }

            var tracker = trackerTemplate.Build(new ContentControl());

            if (tracker.Result != this.currentTracker)
            {
                this.HideTracker();
                this.overlays.Children.Add(tracker.Result);
                this.currentTracker = tracker.Result;
            }

            if (this.currentTracker != null)
            {
                this.currentTracker.DataContext = trackerHitResult;
            }
        }

        /// <summary>
        /// Shows the zoom rectangle.
        /// </summary>
        /// <param name="r">The rectangle.</param>
        public void ShowZoomRectangle(OxyRect r)
        {
            this.zoomControl.Width = r.Width;
            this.zoomControl.Height = r.Height;
            Canvas.SetLeft(this.zoomControl, r.Left);
            Canvas.SetTop(this.zoomControl, r.Top);
            this.zoomControl.Template = this.ZoomRectangleTemplate;
            this.zoomControl.IsVisible = true;
        }

        /// <summary>
        /// Stores text on the clipboard.
        /// </summary>
        /// <param name="text">The text.</param>
        public async void SetClipboardText(string text)
        {
            if (TopLevel.GetTopLevel(this) is { Clipboard: { } clipboard })
            {
                await clipboard.SetTextAsync(text).ConfigureAwait(true);
            }
        }

        protected void UpdatePlot()
        {
            if (this.ActualModel is PlotModel plotModel)
            {
                var updateRequired = Interlocked.Exchange(ref this.isUpdateRequired, 0);
                if (updateRequired == 0)
                {
                    return;
                }

                lock (plotModel.SyncRoot)
                {
                    ((IPlotModel)plotModel).Update(updateRequired > 1);
                }
            }
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            this.panel = e.NameScope.Find(PartPanel) as Panel;
            if (this.panel == null)
            {
                return;
            }

            this.overlays = new Canvas { Name = "Overlays" };
            this.panel.Children.Add(this.overlays);

            this.zoomControl = new ContentControl();
            this.overlays.Children.Add(this.zoomControl);
        }

        private void OnSizeChanged(Rect size)
        {
            if (size.Height > 0 && size.Width > 0)
            {
                this.InvalidatePlot(false);
            }
        }
    }
}
