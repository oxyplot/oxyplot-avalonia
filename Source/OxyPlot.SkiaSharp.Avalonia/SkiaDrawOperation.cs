using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Linq;

namespace OxyPlot.Avalonia
{
    public abstract class SkiaDrawOperation(Rect bounds) : ICustomDrawOperation
    {
        public Rect Bounds { get; } = bounds;

        private IImmutableGlyphRunReference noSkiaGlyphRun;
        private IImmutableGlyphRunReference NoSkiaGlyphRun => this.noSkiaGlyphRun ??= GetNoSkiaGlyphRun();
        protected abstract void Render(SKCanvas canvas);

        private static IImmutableGlyphRunReference GetNoSkiaGlyphRun()
        {
            var text = "SkiaDrawOperation can only be used if Skia rendering API is used.";
            var glyphs = text.Select(ch => Typeface.Default.GlyphTypeface.GetGlyph(ch)).ToArray();
            return new GlyphRun(Typeface.Default.GlyphTypeface, 12, text.AsMemory(), glyphs).TryCreateImmutableGlyphRunReference();
        }

        void ICustomDrawOperation.Render(ImmediateDrawingContext context)
        {
            var skiaSharpLeaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (skiaSharpLeaseFeature is null)
            {
                context.DrawGlyphRun(Brushes.Black, this.NoSkiaGlyphRun);
            }
            else
            {
                using var lease = skiaSharpLeaseFeature.Lease();
                this.Render(lease.SkCanvas);
            }
        }

        void IDisposable.Dispose()
        {
            // we don't hold any unmanaged resources
            GC.SuppressFinalize(this);
        }

        bool ICustomDrawOperation.HitTest(Point p)
        {
            return false;
        }

        bool IEquatable<ICustomDrawOperation>.Equals(ICustomDrawOperation other)
        {
            return false;
        }
    }
}
