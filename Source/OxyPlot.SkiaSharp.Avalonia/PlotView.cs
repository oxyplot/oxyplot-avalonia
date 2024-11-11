using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using OxyPlot.Avalonia;
using System.Threading;

namespace OxyPlot.SkiaSharp.Avalonia
{
    public class PlotView : PlotBase
    {
        private readonly PlotRenderer plotRenderer;

        public PlotView()
        {
            this.plotRenderer = new PlotRenderer(this);
        }

        public override void InvalidatePlot(bool updateData = true)
        {
            base.InvalidatePlot(updateData);

            // do plot update on the UI Thread
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.UpdatePlotIfRequired();
                // this check prevents us from calling InvalidateVisual multiple times when the plot is invalidated in quick succession
                // InvalidateVisual will eventually cause PlotRenderer.Render to be executed 
                if (Interlocked.Exchange(ref this.isRenderRequired, 1) == 0)
                {
                    this.plotRenderer.InvalidateVisual();
                }

            }, DispatcherPriority.Background);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            this.panel.Children.Insert(0, plotRenderer);
        }
    }
}
