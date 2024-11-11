using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Skia;
using Avalonia.Threading;
using OxyPlot.Avalonia;
using SkiaSharp;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace OxyPlot.SkiaSharp.Avalonia.PictureRecorder
{
    public sealed class PlotRenderer(PlotView parent) : Control, IDisposable
    {
        private readonly SkiaRenderContext renderContext = new();
        private Exception renderException;
        private SKPicture currentPicture;
        private CancellationTokenSource renderCancellationTokenSource;
        private readonly SemaphoreSlim renderRequiredEvent = new(0);
        private readonly SemaphoreSlim renderLoopMutex = new(1, 1);

        public PlotView PlotView { get; } = parent;

        /// <summary>
        /// Notifies the <see cref="PlotRenderer"/> that a re-render is required.
        /// </summary>
        public void RequestRender()
        {
            this.renderRequiredEvent.Release();
        }

        /// <inheritdoc />
        public override void Render(DrawingContext context)
        {
            if (this.renderException is not null)
            {
                var exceptionText = new FormattedText(
                    this.renderException.ToString(),
                    CultureInfo.CurrentCulture,
                    CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                    Typeface.Default,
                    10,
                    Brushes.Black);

                context.DrawText(exceptionText, new Point(20, 20));
                return;
            }

            using var drawOperation = new SKPictureDrawOperation(new Rect(0, 0, this.Bounds.Width, this.Bounds.Height), this);
            context.Custom(drawOperation);
        }

        /// <summary>
        /// This loop runs until canceled and updates and renders the plot if required.
        /// </summary>
        private async Task RenderLoopAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await this.renderRequiredEvent.WaitAsync(cancellationToken);
                while (this.renderRequiredEvent.CurrentCount > 0)
                {
                    await this.renderRequiredEvent.WaitAsync(cancellationToken);
                }

                var size = this.Bounds.Size;

                if (size.Width > 0 && size.Height > 0 && this.PlotView.ActualModel is PlotModel plotModel)
                {
                    var isUpdateRequired = Interlocked.Exchange(ref this.PlotView.isUpdateRequired, 0);
                    if (isUpdateRequired > 0)
                    {
                        // plot update and render might be CPU-intensive, so run it on a background thread
                        await Task.Run(() =>
                        {
                            lock (plotModel.SyncRoot)
                            {
                                var iPlotModel = (IPlotModel)plotModel;
                                iPlotModel.Update(isUpdateRequired > 1);
                                cancellationToken.ThrowIfCancellationRequested();
                                this.Render(iPlotModel, size);
                            }
                        }, cancellationToken);
                    }
                }
            }
        }

        private void Render(IPlotModel model, Size size)
        {
            using var recorder = new SKPictureRecorder();
            var rect = new Rect(size);
            using var canvas = recorder.BeginRecording(rect.ToSKRect());

            this.renderContext.SkCanvas = canvas;

            if (model.Background.IsVisible())
            {
                canvas.Clear(model.Background.ToSKColor());
            }

            model.Render(this.renderContext, new OxyRect(0, 0, size.Width, size.Height));

            // TODO Somehow dispose of old image
            this.currentPicture = recorder.EndRecording();
            Dispatcher.UIThread.InvokeAsync(this.InvalidateVisual);
        }

        /// <inheritdoc />
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            // We deliberately just 'fire and forget' the render loop. It will run forever until canceled via renderCancellationTokenSource.
            // Potential exceptions are stored in renderException.
            _ = this.StartRenderLoopAsync();
        }

        /// <inheritdoc />
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            this.StopRenderLoop();
        }

        private async Task StartRenderLoopAsync()
        {
            await this.renderLoopMutex.WaitAsync();
            this.renderException = null;
            this.renderCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await this.RenderLoopAsync(this.renderCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                this.renderException = e;
            }
            finally
            {
                this.renderLoopMutex.Release();
            }
        }

        private void StopRenderLoop()
        {
            this.renderCancellationTokenSource?.Cancel();
            this.renderCancellationTokenSource?.Dispose();
            this.renderCancellationTokenSource = null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // First stop the render loop
            this.StopRenderLoop();

            // wait until renderLoopMutex is free -> this means the render loop has terminated
            this.renderLoopMutex.Wait();

            this.renderContext.Dispose();
            this.renderLoopMutex.Dispose();
            this.renderRequiredEvent.Dispose();

            GC.SuppressFinalize(this);
        }

        private class SKPictureDrawOperation(Rect bounds, PlotRenderer parent) : SkiaDrawOperation(bounds)
        {
            public PlotRenderer Parent { get; } = parent;

            protected override void Render(SKCanvas canvas)
            {
                if (this.Parent.currentPicture is not null)
                {
                    canvas.DrawPicture(this.Parent.currentPicture);
                }
            }
        }
    }
}
