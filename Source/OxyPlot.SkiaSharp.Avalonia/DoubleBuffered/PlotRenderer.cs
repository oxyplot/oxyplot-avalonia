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

namespace OxyPlot.SkiaSharp.Avalonia.DoubleBuffered
{
    public sealed class PlotRenderer(PlotView parent) : Control, IDisposable
    {
        private readonly SkiaRenderContext renderContext = new();
        private Exception renderException;
        private readonly object frontBufferLock = new();
        private SKBitmap frontBuffer;
        private SKBitmap backBuffer;
        private CancellationTokenSource renderCancellationTokenSource;
        private readonly AutoResetEvent renderRequiredEvent = new(false);
        private readonly Mutex renderLoopMutex = new();

        public PlotView PlotView { get; } = parent;

        /// <summary>
        /// Notifies the <see cref="PlotRenderer"/> that a re-render is required.
        /// </summary>
        public void RequestRender()
        {
            this.renderRequiredEvent.Set();
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

            using var drawOperation = new DoubleBufferDrawOperation(new Rect(0, 0, this.Bounds.Width, this.Bounds.Height), this);
            context.Custom(drawOperation);
        }

        private void BufferSwitch()
        {
            lock (this.frontBufferLock)
            {
                (this.frontBuffer, this.backBuffer) = (this.backBuffer, this.frontBuffer);
            }
        }

        /// <summary>
        /// Makes sure that the backbuffer is initialized with the required size.
        /// </summary>
        /// <param name="size">The size (in device-independent pixels).</param>
        /// <returns>The DPI scaling factor.</returns>
        private double EnsureBackBuffer(Size size)
        {
            var dpiScale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
            var width = (int)(size.Width * dpiScale);
            var height = (int)(size.Height * dpiScale);
            if (this.backBuffer is null || this.backBuffer.Width != width || this.backBuffer.Height != height)
            {
                this.backBuffer?.Dispose();
                this.backBuffer = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            }

            return dpiScale;
        }

        /// <summary>
        /// This loop runs until canceled and updates and renders the plot if required.
        /// </summary>
        private void RenderLoop(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                this.renderRequiredEvent.WaitOne();
                cancellationToken.ThrowIfCancellationRequested();
                var size = this.Bounds.Size;

                if (size.Width > 0 && size.Height > 0 && this.PlotView.ActualModel is PlotModel plotModel)
                {
                    var isUpdateRequired = Interlocked.Exchange(ref this.PlotView.isUpdateRequired, 0);
                    var iPlotModel = (IPlotModel)plotModel;
                    if (isUpdateRequired > 0)
                    {
                        lock (plotModel.SyncRoot)
                        {
                            iPlotModel.Update(isUpdateRequired > 1);
                            cancellationToken.ThrowIfCancellationRequested();
                            this.Render(iPlotModel, size);
                        }
                    }
                    else if (Interlocked.Exchange(ref this.PlotView.isRenderRequired, 0) == 1)
                    {
                        lock (plotModel.SyncRoot)
                        {
                            this.Render(iPlotModel, size);
                        }
                    }
                }
            }
        }

        private void Render(IPlotModel model, Size size)
        {
            var scale = this.EnsureBackBuffer(size);

            using (var canvas = new SKCanvas(this.backBuffer))
            {
                canvas.Scale((float)scale);
                this.renderContext.SkCanvas = canvas;
                canvas.Clear(model.Background.ToSKColor());
                model.Render(this.renderContext, new OxyRect(0, 0, this.backBuffer.Width / scale, this.backBuffer.Height / scale));
            }

            this.BufferSwitch();
            Dispatcher.UIThread.InvokeAsync(this.InvalidateVisual);
        }

        /// <inheritdoc />
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            this.StartRenderLoop();
        }

        /// <inheritdoc />
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            this.StopRenderLoop();
        }

        private void StartRenderLoop()
        {
            new Task(() =>
            {
                this.renderLoopMutex.WaitOne();
                this.renderException = null;
                this.renderCancellationTokenSource = new CancellationTokenSource();

                try
                {
                    this.RenderLoop(this.renderCancellationTokenSource.Token);
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
                    this.backBuffer?.Dispose();
                    this.frontBuffer?.Dispose();
                    this.frontBuffer = this.backBuffer = null;
                    this.renderLoopMutex.ReleaseMutex();
                }
            }, TaskCreationOptions.LongRunning).Start();
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

            // release renderRequiredEvent once more in case the render loop currently is waiting for the mutex; otherwise it might wait forever and will not be canceled
            this.RequestRender();

            // wait until renderLoopMutex is free -> this means the render loop has terminated
            this.renderLoopMutex.WaitOne();

            // the buffers should be disposed at this point, but let's make sure
            this.backBuffer?.Dispose();
            this.frontBuffer?.Dispose();

            this.renderContext.Dispose();
            this.renderLoopMutex.Dispose();
            this.renderRequiredEvent.Dispose();

            GC.SuppressFinalize(this);
        }

        private class DoubleBufferDrawOperation(Rect bounds, PlotRenderer parent) : SkiaDrawOperation(bounds)
        {
            public PlotRenderer Parent { get; } = parent;

            protected override void Render(SKCanvas canvas)
            {
                lock (this.Parent.frontBufferLock)
                {
                    if (this.Parent.frontBuffer is not null)
                    {
                        canvas.DrawBitmap(this.Parent.frontBuffer, this.Bounds.ToSKRect());
                    }
                }
            }
        }
    }
}
