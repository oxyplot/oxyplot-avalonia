using Avalonia.Controls.Primitives;
using OxyPlot.Avalonia;

namespace OxyPlot.SkiaSharp.Avalonia.PictureRecorder
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
            // At this point we only notify PlotRenderer that an update needs to be done.
            // Plot update and/or rendering will be done by PlotRenderer on a background thread as necessary. 
            this.plotRenderer.RequestRender();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            this.panel.Children.Insert(0, plotRenderer);
        }
    }
}
