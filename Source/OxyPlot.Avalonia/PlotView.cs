// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Plot.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Represents a control that displays a <see cref="PlotModel" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using System.Threading;

namespace OxyPlot.Avalonia
{
    public partial class PlotView : PlotBase
    {
        /// <summary>
        /// The render context
        /// </summary>
        private CanvasRenderContext renderContext;

        /// <summary>
        /// The canvas.
        /// </summary>
        private Canvas canvas;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlotBase" /> class.
        /// </summary>
        public PlotView()
        {
            this.DisconnectCanvasWhileUpdating = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to disconnect the canvas while updating.
        /// </summary>
        /// <value><c>true</c> if canvas should be disconnected while updating; otherwise, <c>false</c>.</value>
        public bool DisconnectCanvasWhileUpdating { get; set; }

        /// <inheritdoc />
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            this.canvas = new Canvas();
            this.panel.Children.Insert(0, this.canvas);
            this.renderContext = new CanvasRenderContext(this.canvas);
        }

        /// <inheritdoc />
        public override void InvalidatePlot(bool updateData = true)
        {
            base.InvalidatePlot(updateData);
            // do plot update on the UI Thread, but with 'Background' priority, so it doesn't block UI
            Dispatcher.UIThread.InvokeAsync(() => 
            {
                this.UpdatePlot();
                this.InvalidateArrange(); 
            }, DispatcherPriority.Background);
        }

        /// <inheritdoc />
        protected override Size ArrangeOverride(Size finalSize)
        {
            var actualSize = base.ArrangeOverride(finalSize);
            if (actualSize.Width > 0 && actualSize.Height > 0)
            {
                this.UpdateVisuals();
            }

            return actualSize;
        }

        private void UpdateVisuals()
        {
            if (this.canvas == null || this.renderContext == null)
            {
                return;
            }

            if (!this.IsEffectivelyVisible)
            {
                return;
            }

            // Clear the canvas
            this.canvas.Children.Clear();

            if (this.ActualModel?.Background.IsVisible() == true)
            {
                this.canvas.Background = this.ActualModel.Background.ToBrush();
            }
            else
            {
                this.canvas.Background = null;
            }

            if (this.ActualModel is PlotModel plotModel)
            {
                lock (plotModel.SyncRoot)
                {
                    var updateState = Interlocked.Exchange(ref this.isUpdateRequired, 0);

                    if (updateState > 0)
                    {
                        ((IPlotModel)plotModel).Update(updateState > 1);
                    }

                    if (this.DisconnectCanvasWhileUpdating)
                    {
                        // TODO: profile... not sure if this makes any difference
                        var idx = this.panel.Children.IndexOf(this.canvas);
                        if (idx != -1)
                        {
                            this.panel.Children.RemoveAt(idx);
                        }

                        ((IPlotModel)plotModel).Render(this.renderContext, new OxyRect(0, 0, this.canvas.Bounds.Width, this.canvas.Bounds.Height));

                        // reinsert the canvas again
                        if (idx != -1)
                        {
                            this.panel.Children.Insert(idx, this.canvas);
                        }
                    }
                    else
                    {
                        ((IPlotModel)plotModel).Render(this.renderContext, new OxyRect(0, 0, this.canvas.Bounds.Width, this.canvas.Bounds.Height));
                    }
                }
            }
        }
    }
}
