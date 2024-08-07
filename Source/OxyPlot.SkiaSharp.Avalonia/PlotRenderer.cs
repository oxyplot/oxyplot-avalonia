using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using OxyPlot.Avalonia;
using OxyPlot.Avalonia.Extensions;
using SkiaSharp;
using System;

namespace OxyPlot.SkiaSharp.Avalonia
{
    public class PlotRenderer(PlotView plotView) : Control, IDisposable
    {
        private readonly SkiaRenderContext renderContext = new();

        public PlotView PlotView { get; } = plotView;

        public override void Render(DrawingContext context)
        {
            if (this.PlotView.ActualModel is not PlotModel plotModel || plotModel.Background.IsInvisible())
            {
                context.FillRectangle(this.PlotView.Background, this.Bounds);
            }

            using var drawOperation = new SkiaPlotDrawOperation(new Rect(0, 0, this.Bounds.Width, this.Bounds.Height), this);
            context.Custom(drawOperation);
        }

        private class SkiaPlotDrawOperation(Rect bounds, PlotRenderer parent) : SkiaDrawOperation(bounds)
        {
            public PlotRenderer Parent { get; } = parent;

            protected override void Render(SKCanvas canvas)
            {
                if (this.Parent.PlotView.ActualModel is PlotModel plotModel)
                {
                    this.Parent.renderContext.SkCanvas = canvas;

                    lock (plotModel.SyncRoot)
                    {
                        if (plotModel.Background.IsVisible())
                        {
                            canvas.Clear(plotModel.Background.ToSKColor());
                        }    

                        ((IPlotModel)plotModel).Render(this.Parent.renderContext, this.Bounds.ToOxyRect());
                    }
                }
            }
        }

        public void Dispose()
        {
            this.renderContext.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
