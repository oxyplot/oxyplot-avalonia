using Avalonia.Controls.Primitives;
using OxyPlot.Avalonia;

namespace OxyPlot.SkiaSharp.Avalonia.DoubleBuffered
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
            // Update is done on the render thread, so it doesn't block the UI Thread
            this.plotRenderer.RequestRender();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            this.panel.Children.Insert(0, plotRenderer);
        }
    }
}
