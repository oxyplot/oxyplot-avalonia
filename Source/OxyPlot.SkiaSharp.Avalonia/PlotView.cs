using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using OxyPlot.Avalonia;

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

            // do plot update on the UI Thread, but with 'Background' priority, so it doesn't block UI
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.UpdatePlot();
                this.plotRenderer.InvalidateVisual();
            }, DispatcherPriority.Background);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            this.panel.Children.Insert(0, plotRenderer);
        }
    }
}
