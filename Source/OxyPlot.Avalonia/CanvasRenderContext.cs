// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CanvasRenderContext.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Implements <see cref="IRenderContext" /> for <see cref="System.Windows.Controls.Canvas" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OxyPlot.Avalonia
{
    using global::Avalonia;
    using global::Avalonia.Controls;
    using global::Avalonia.Controls.Shapes;
    using global::Avalonia.Media;
    using global::Avalonia.Media.Imaging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Path = global::Avalonia.Controls.Shapes.Path;

    /// <summary>
    /// Implements <see cref="IRenderContext" /> for <see cref="Windows.Controls.Canvas" />.
    /// </summary>
    public class CanvasRenderContext : ClippingRenderContext
    {
        /// <summary>
        /// The maximum number of figures per geometry.
        /// </summary>
        private const int MaxFiguresPerGeometry = 16;

        /// <summary>
        /// The maximum number of polylines per line.
        /// </summary>
        private const int MaxPolylinesPerLine = 64;

        /// <summary>
        /// The minimum number of points per polyline.
        /// </summary>
        private const int MinPointsPerPolyline = 16;

        /// <summary>
        /// The images in use
        /// </summary>
        private readonly HashSet<OxyImage> imagesInUse = new HashSet<OxyImage>();

        /// <summary>
        /// The image cache
        /// </summary>
        private readonly Dictionary<OxyImage, IBitmap> imageCache = new Dictionary<OxyImage, IBitmap>();

        /// <summary>
        /// The brush cache.
        /// </summary>
        private readonly Dictionary<OxyColor, IBrush> brushCache = new Dictionary<OxyColor, IBrush>();

        /// <summary>
        /// The canvas.
        /// </summary>
        private readonly Canvas canvas;

        /// <summary>
        /// The clip rectangle.
        /// </summary>
        private Rect? clip;

        /// <summary>
        /// The current tool tip
        /// </summary>
        private string currentToolTip;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasRenderContext" /> class.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        public CanvasRenderContext(Canvas canvas)
        {
            this.canvas = canvas;
            UseStreamGeometry = true;
            RendersToScreen = true;
            BalancedLineDrawingThicknessLimit = 3.5;
        }

        /// <summary>
        /// Gets or sets the thickness limit for "balanced" line drawing.
        /// </summary>
        public double BalancedLineDrawingThicknessLimit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use stream geometry for lines and polygons rendering.
        /// </summary>
        /// <value><c>true</c> if stream geometry should be used; otherwise, <c>false</c> .</value>
        /// <remarks>The XamlWriter does not serialize StreamGeometry, so set this to <c>false</c> if you want to export to XAML. Using stream geometry seems to be slightly faster than using path geometry.</remarks>
        public bool UseStreamGeometry { get; set; }

        #region DrawEllipse(s)
        ///<inheritdoc/>
        public override void DrawEllipse(OxyRect rect,
            OxyColor fill,
            OxyColor stroke,
            double thickness,
            EdgeRenderingMode edgeRenderingMode)
        {
            var path = CreateAndAdd<Path>();
            SetStroke(path, stroke, thickness, edgeRenderingMode);
            if (!fill.IsUndefined())
            {
                path.Fill = GetCachedBrush(fill);
            }
            path.Data = new EllipseGeometry(ToRect(rect));
        }

        private void DrawEllipsesByStreamGeometry(
            IList<OxyRect> rects,
            OxyColor fill,
            OxyColor stroke,
            double thickness,
            EdgeRenderingMode edgeRenderingMode,
            double[] dashArray,
            LineJoin lineJoin)
        {
            const double ratio = 0.55228475; // (Math.Sqrt(2) - 1.0) * 4.0 / 3.0;

            Path path = null;
            StreamGeometry streamGeometry = null;
            StreamGeometryContext sgc = null;
            var count = 0;

            bool isSolid = !fill.IsUndefined();
            IBrush cachedBrush = null;
            if (isSolid)
            {
                cachedBrush = GetCachedBrush(fill);
            }

            foreach (var rect in rects)
            {
                if (path == null)
                {
                    path = CreateAndAdd<Path>();
                    SetStroke(path, stroke, thickness, edgeRenderingMode, lineJoin, dashArray, 0);
                    if (isSolid)
                    {
                        path.Fill = cachedBrush;
                    }

                    streamGeometry = new StreamGeometry();
                    sgc = streamGeometry.Open();
                    sgc.SetFillRule(FillRule.NonZero);
                }

                var a = rect.Width / 2.0;
                var b = rect.Height / 2.0;

                var x0 = rect.Center.X - a;
                var x1 = rect.Center.X - a * ratio;
                var x2 = rect.Center.X;
                var x3 = rect.Center.X + a * ratio;
                var x4 = rect.Center.X + a;

                var y0 = rect.Center.Y - b;
                var y1 = rect.Center.Y - b * ratio;
                var y2 = rect.Center.Y;
                var y3 = rect.Center.Y + b * ratio;
                var y4 = rect.Center.Y + b;

                sgc.BeginFigure(new Point(x2, y0), isSolid);
                sgc.CubicBezierTo(new Point(x3, y0), new Point(x4, y1), new Point(x4, y2));
                sgc.CubicBezierTo(new Point(x4, y3), new Point(x3, y4), new Point(x2, y4));
                sgc.CubicBezierTo(new Point(x1, y4), new Point(x0, y3), new Point(x0, y2));
                sgc.CubicBezierTo(new Point(x0, y1), new Point(x1, y0), new Point(x2, y0));
                sgc.EndFigure(true);

                count++;

                // Must limit the number of figures, otherwise drawing errors...
                if (count > MaxFiguresPerGeometry)
                {
                    sgc.Dispose();
                    path.Data = streamGeometry;
                    path = null;
                    count = 0;
                }
            }

            if (path != null)
            {
                sgc.Dispose();
                path.Data = streamGeometry;
            }
        }

        ///<inheritdoc/>
        public override void DrawEllipses(IList<OxyRect> rectangles,
            OxyColor fill,
            OxyColor stroke,
            double thickness,
            EdgeRenderingMode edgeRenderingMode)
        {
            if (UseStreamGeometry)
            {
                DrawEllipsesByStreamGeometry(rectangles, fill, stroke, thickness, edgeRenderingMode, null, LineJoin.Miter);
                return;
            }

            foreach (var rect in rectangles)
            {
                DrawEllipse(rect, fill, stroke, thickness, edgeRenderingMode);
            }
        }
        #endregion

        ///<inheritdoc/>
        public override void DrawLine(
            IList<ScreenPoint> points,
            OxyColor stroke,
            double thickness,
            EdgeRenderingMode edgeRenderingMode,
            double[] dashArray,
            LineJoin lineJoin)
        {
            if (thickness < BalancedLineDrawingThicknessLimit)
            {
                DrawLineBalanced(points, stroke, thickness, edgeRenderingMode, dashArray, lineJoin);
                return;
            }

            var e = CreateAndAdd<Polyline>();
            SetStroke(e, stroke, thickness, edgeRenderingMode, lineJoin, dashArray, 0);

            bool aliased = !ShouldUseAntiAliasingForLine(edgeRenderingMode, points);
            e.Points = ToPointCollection(points, aliased);
        }

        #region DrawLineSegments
        /// <summary>
        /// Draws the line segments by stream geometry.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="stroke">The stroke color.</param>
        /// <param name="thickness">The thickness.</param>
        /// <param name="edgeRenderingMode">The edge rendering mode.</param>
        /// <param name="dashArray">The dash array. Use <c>null</c> to get a solid line.</param>
        /// <param name="lineJoin">The line join.</param>
        private void DrawLineSegmentsByStreamGeometry(
            IList<ScreenPoint> points,
            OxyColor stroke,
            double thickness,
            EdgeRenderingMode edgeRenderingMode,
            double[] dashArray,
            LineJoin lineJoin)
        {
            bool aliased = edgeRenderingMode == EdgeRenderingMode.PreferSpeed;

            StreamGeometry streamGeometry = null;
            StreamGeometryContext streamGeometryContext = null;

            var count = 0;

            for (int i = 0; i + 1 < points.Count; i += 2)
            {
                if (streamGeometry == null)
                {
                    streamGeometry = new StreamGeometry();
                    streamGeometryContext = streamGeometry.Open();
                }

                streamGeometryContext.BeginFigure(ToPoint(points[i], aliased), false);
                streamGeometryContext.LineTo(ToPoint(points[i + 1], aliased));

                count++;

                // Must limit the number of figures, otherwise drawing errors...
                if (count > MaxFiguresPerGeometry || dashArray != null)
                {
                    streamGeometryContext.Dispose();
                    var path = CreateAndAdd<Path>();
                    SetStroke(path, stroke, thickness, edgeRenderingMode, lineJoin, dashArray, 0);
                    path.Data = streamGeometry;
                    streamGeometry = null;
                    count = 0;
                }
            }

            if (streamGeometry != null)
            {
                streamGeometryContext.Dispose();
                var path = CreateAndAdd<Path>();
                SetStroke(path, stroke, thickness, edgeRenderingMode, lineJoin, null, 0);
                path.Data = streamGeometry;
            }
        }

        /// <summary>
        /// Draws line segments defined by points (0,1) (2,3) (4,5) etc.
        /// This should have better performance than calling DrawLine for each segment.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="stroke">The stroke color.</param>
        /// <param name="thickness">The stroke thickness (in device independent units, 1/96 inch).</param>
        /// <param name="edgeRenderingMode">The edge rendering mode.</param>
        /// <param name="dashArray">The dash array (in device independent units, 1/96 inch).</param>
        /// <param name="lineJoin">The line join type.</param>
        public override void DrawLineSegments(
            IList<ScreenPoint> points,
            OxyColor stroke,
            double thickness,
            EdgeRenderingMode edgeRenderingMode,
            double[] dashArray,
            LineJoin lineJoin)
        {
            if (UseStreamGeometry)
            {
                DrawLineSegmentsByStreamGeometry(points, stroke, thickness, edgeRenderingMode, dashArray, lineJoin);
                return;
            }

            bool aliased = edgeRenderingMode == EdgeRenderingMode.PreferSpeed;

            Path path = null;
            PathGeometry pathGeometry = null;

            var count = 0;

            for (int i = 0; i + 1 < points.Count; i += 2)
            {
                if (path == null)
                {
                    path = CreateAndAdd<Path>();
                    SetStroke(path, stroke, thickness, edgeRenderingMode, lineJoin, dashArray, 0);
                    pathGeometry = new PathGeometry();
                }

                var figure = new PathFigure { StartPoint = ToPoint(points[i], aliased), IsClosed = false };
                figure.Segments.Add(new LineSegment { Point = ToPoint(points[i + 1], aliased) });
                pathGeometry.Figures.Add(figure);

                count++;

                // Must limit the number of figures, otherwise drawing errors...
                if (count > MaxFiguresPerGeometry || dashArray != null)
                {
                    path.Data = pathGeometry;
                    path = null;
                    count = 0;
                }
            }

            if (path != null)
            {
                path.Data = pathGeometry;
            }
        }
        #endregion

        #region DrawPolygon(s)
        ///<inheritdoc/>
        public override void DrawPolygon(
            IList<ScreenPoint> points,
            OxyColor fill,
            OxyColor stroke,
            double thickness,
            EdgeRenderingMode edgeRenderingMode,
            double[] dashArray,
            LineJoin lineJoin)
        {
            var e = CreateAndAdd<Polygon>();
            SetStroke(e, stroke, thickness, edgeRenderingMode, lineJoin, dashArray, 0);

            if (!fill.IsUndefined())
            {
                e.Fill = GetCachedBrush(fill);
            }

            bool aliased = edgeRenderingMode == EdgeRenderingMode.PreferSpeed;
            e.Points = ToPointCollection(points, aliased);
        }

        private void DrawPolygonsByStreamGeometry(
            IList<IList<ScreenPoint>> polygons,
            OxyColor fill,
            OxyColor stroke,
            double thickness,
            EdgeRenderingMode edgeRenderingMode,
            double[] dashArray,
            LineJoin lineJoin)
        {
            Path path = null;
            StreamGeometry streamGeometry = null;
            StreamGeometryContext sgc = null;
            var count = 0;

            Func<ScreenPoint, Point> toPointFunc;
            if (edgeRenderingMode == EdgeRenderingMode.PreferSpeed) // aliased
            {
                toPointFunc = ToPixelAlignedPoint;
            }
            else
            {
                toPointFunc = ToPoint;
            }

            bool isSolid = !fill.IsUndefined();
            IBrush cachedBrush = null;
            if (isSolid)
            {
                cachedBrush = GetCachedBrush(fill);
            }

            foreach (var polygon in polygons)
            {
                if (path == null)
                {
                    path = CreateAndAdd<Path>();
                    SetStroke(path, stroke, thickness, edgeRenderingMode, lineJoin, dashArray, 0);
                    if (isSolid)
                    {
                        path.Fill = cachedBrush;
                    }

                    streamGeometry = new StreamGeometry();
                    sgc = streamGeometry.Open();
                    sgc.SetFillRule(FillRule.NonZero);
                }

                var first = true;
                foreach (var p in polygon)
                {
                    if (first)
                    {
                        sgc.BeginFigure(toPointFunc(p), isSolid);
                        first = false;
                    }
                    else
                    {
                        sgc.LineTo(toPointFunc(p));
                    }
                }

                sgc.EndFigure(true);

                count++;

                // Must limit the number of figures, otherwise drawing errors...
                if (count > MaxFiguresPerGeometry)
                {
                    sgc.Dispose();
                    path.Data = streamGeometry;
                    path = null;
                    count = 0;
                }
            }

            if (path != null)
            {
                sgc.Dispose();
                path.Data = streamGeometry;
            }
        }

        ///<inheritdoc/>
        public override void DrawPolygons(
            IList<IList<ScreenPoint>> polygons,
            OxyColor fill,
            OxyColor stroke,
            double thickness,
            EdgeRenderingMode edgeRenderingMode,
            double[] dashArray,
            LineJoin lineJoin)
        {
            if (UseStreamGeometry)
            {
                DrawPolygonsByStreamGeometry(polygons, fill, stroke, thickness, edgeRenderingMode, dashArray, lineJoin);
                return;
            }

            Path path = null;
            PathGeometry pathGeometry = null;
            var count = 0;

            Func<ScreenPoint, Point> toPointFunc;
            if (edgeRenderingMode == EdgeRenderingMode.PreferSpeed) // aliased
            {
                toPointFunc = ToPixelAlignedPoint;
            }
            else
            {
                toPointFunc = ToPoint;
            }

            bool isSolid = !fill.IsUndefined();
            IBrush cachedBrush = null;
            if (isSolid)
            {
                cachedBrush = GetCachedBrush(fill);
            }

            foreach (var polygon in polygons)
            {
                if (path == null)
                {
                    path = CreateAndAdd<Path>();
                    SetStroke(path, stroke, thickness, edgeRenderingMode, lineJoin, dashArray, 0);
                    if (isSolid)
                    {
                        path.Fill = cachedBrush;
                    }

                    pathGeometry = new PathGeometry { FillRule = FillRule.NonZero };
                }

                PathFigure figure = null;
                var first = true;
                foreach (var p in polygon)
                {
                    if (first)
                    {
                        figure = new PathFigure
                        {
                            StartPoint = toPointFunc(p),
                            IsFilled = isSolid,
                            IsClosed = true
                        };
                        pathGeometry.Figures.Add(figure);

                        first = false;
                    }
                    else
                    {
                        figure.Segments.Add(new LineSegment { Point = toPointFunc(p) });
                    }
                }

                count++;

                // Must limit the number of figures, otherwise drawing errors...
                if (count > MaxFiguresPerGeometry)
                {
                    path.Data = pathGeometry;
                    path = null;
                    count = 0;
                }
            }

            if (path != null)
            {
                path.Data = pathGeometry;
            }
        }
        #endregion

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="fill">The fill color. If set to <c>OxyColors.Undefined</c>, the rectangle will not be filled.</param>
        /// <param name="stroke">The stroke color. If set to <c>OxyColors.Undefined</c>, the rectangle will not be stroked.</param>
        /// <param name="thickness">The stroke thickness (in device independent units, 1/96 inch).</param>
        public override void DrawRectangle(OxyRect rect,
            OxyColor fill,
            OxyColor stroke,
            double thickness,
            EdgeRenderingMode edgeRenderingMode)
        {
            var path = CreateAndAdd<Path>();
            SetStroke(path, stroke, thickness, edgeRenderingMode);
            if (!fill.IsUndefined())
            {
                path.Fill = GetCachedBrush(fill);
            }
            path.Data = new RectangleGeometry(ToRect(rect));
        }

        /// <summary>
        /// Draws a collection of rectangles, where all have the same stroke and fill.
        /// This performs better than calling DrawRectangle multiple times.
        /// </summary>
        /// <param name="rectangles">The rectangles.</param>
        /// <param name="fill">The fill color. If set to <c>OxyColors.Undefined</c>, the rectangles will not be filled.</param>
        /// <param name="stroke">The stroke color. If set to <c>OxyColors.Undefined</c>, the rectangles will not be stroked.</param>
        /// <param name="thickness">The stroke thickness (in device independent units, 1/96 inch).</param>
        public override void DrawRectangles(IList<OxyRect> rectangles,
            OxyColor fill,
            OxyColor stroke,
            double thickness,
            EdgeRenderingMode edgeRenderingMode)
        {
            var polys = rectangles.Select(r => RectangleToPolygon(r)).ToList();
            DrawPolygons(polys, fill, stroke, thickness, edgeRenderingMode, null, LineJoin.Miter);
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="p">The position.</param>
        /// <param name="text">The text.</param>
        /// <param name="fill">The text color.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">Size of the font (in device independent units, 1/96 inch).</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <param name="rotate">The rotation angle.</param>
        /// <param name="halign">The horizontal alignment.</param>
        /// <param name="valign">The vertical alignment.</param>
        /// <param name="maxSize">The maximum size of the text (in device independent units, 1/96 inch).</param>
        public override void DrawText(
            ScreenPoint p,
            string text,
            OxyColor fill,
            string fontFamily,
            double fontSize,
            double fontWeight,
            double rotate,
            HorizontalAlignment halign,
            VerticalAlignment valign,
            OxySize? maxSize)
        {
            var tb = CreateAndAdd<TextBlock>();
            tb.Text = text;
            tb.Foreground = GetCachedBrush(fill);
            if (fontFamily != null)
            {
                tb.FontFamily = fontFamily;
            }

            if (fontSize > 0)
            {
                tb.FontSize = fontSize;
            }

            if (fontWeight > 0)
            {
                tb.FontWeight = GetFontWeight(fontWeight);
            }

            double dx = 0;
            double dy = 0;

            if (maxSize != null || halign != HorizontalAlignment.Left || valign != VerticalAlignment.Top)
            {
                tb.Measure(new Size(1000, 1000));
                var size = tb.DesiredSize;
                if (maxSize != null)
                {
                    if (size.Width > maxSize.Value.Width + 1e-3)
                    {
                        size = size.WithWidth(Math.Max(maxSize.Value.Width, 0));
                    }

                    if (size.Height > maxSize.Value.Height + 1e-3)
                    {
                        size = size.WithHeight(Math.Max(maxSize.Value.Height, 0));
                    }

                    tb.Width = size.Width;
                    tb.Height = size.Height;
                }

                if (halign == HorizontalAlignment.Center)
                {
                    dx = -size.Width / 2;
                }

                if (halign == HorizontalAlignment.Right)
                {
                    dx = -size.Width;
                }

                if (valign == VerticalAlignment.Middle)
                {
                    dy = -size.Height / 2;
                }

                if (valign == VerticalAlignment.Bottom)
                {
                    dy = -size.Height;
                }
            }

            var transform = new TransformGroup();
            transform.Children.Add(new TranslateTransform(dx, dy));
            if (Math.Abs(rotate) > double.Epsilon)
            {
                transform.Children.Add(new RotateTransform(rotate));
            }

            transform.Children.Add(new TranslateTransform(p.X, p.Y));
            tb.RenderTransform = transform;
            if (tb.Clip != null)
            {
                tb.Clip.Transform = new MatrixTransform(tb.RenderTransform.Value.Invert());
            }
            tb.RenderTransformOrigin = new RelativePoint(0.0, 0.0, RelativeUnit.Relative);
        }

        /// <summary>
        /// Measures the size of the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">Size of the font (in device independent units, 1/96 inch).</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <returns>
        /// The size of the text (in device independent units, 1/96 inch).
        /// </returns>
        public override OxySize MeasureText(string text,
            string fontFamily,
            double fontSize,
            double fontWeight)
        {
            if (string.IsNullOrEmpty(text))
            {
                return OxySize.Empty;
            }

            var tb = new TextBlock { Text = text };

            if (fontFamily != null)
            {
                tb.FontFamily = fontFamily;
            }

            if (fontSize > 0)
            {
                tb.FontSize = fontSize;
            }

            if (fontWeight > 0)
            {
                tb.FontWeight = GetFontWeight(fontWeight);
            }

            tb.Measure(new Size(1000, 1000));

            return new OxySize(tb.DesiredSize.Width, tb.DesiredSize.Height);
        }

        /// <summary>
        /// Sets the tool tip for the following items.
        /// </summary>
        /// <param name="text">The text in the tool tip.</param>
        public override void SetToolTip(string text)
        {
            currentToolTip = text;
        }

        /// <summary>
        /// Draws a portion of the specified <see cref="OxyImage" />.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="srcX">The x-coordinate of the upper-left corner of the portion of the source image to draw.</param>
        /// <param name="srcY">The y-coordinate of the upper-left corner of the portion of the source image to draw.</param>
        /// <param name="srcWidth">Width of the portion of the source image to draw.</param>
        /// <param name="srcHeight">Height of the portion of the source image to draw.</param>
        /// <param name="destX">The x-coordinate of the upper-left corner of drawn image.</param>
        /// <param name="destY">The y-coordinate of the upper-left corner of drawn image.</param>
        /// <param name="destWidth">The width of the drawn image.</param>
        /// <param name="destHeight">The height of the drawn image.</param>
        /// <param name="opacity">The opacity.</param>
        /// <param name="interpolate">interpolate if set to <c>true</c>.</param>
        public override void DrawImage(
            OxyImage source,
            double srcX,
            double srcY,
            double srcWidth,
            double srcHeight,
            double destX,
            double destY,
            double destWidth,
            double destHeight,
            double opacity,
            bool interpolate)
        {
            if (destWidth <= 0 || destHeight <= 0 || srcWidth <= 0 || srcHeight <= 0)
            {
                return;
            }

            var image = CreateAndAdd<Image>(destX, destY);
            var bitmapChain = GetImageSource(source);

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (srcX == 0 && srcY == 0 && srcWidth == bitmapChain.PixelSize.Width && srcHeight == bitmapChain.PixelSize.Height)
            // ReSharper restore CompareOfFloatsByEqualityOperator
            {
                // do not crop
            }
            else
            {
                image.Clip = new RectangleGeometry(new Rect(srcX, srcY, srcWidth, srcHeight));
            }

            image.Opacity = opacity;
            image.Width = destWidth;
            image.Height = destHeight;
            image.Stretch = Stretch.Fill;

            // Set the position of the image
            Canvas.SetLeft(image, destX);
            Canvas.SetTop(image, destY);
            //// alternative: image.RenderTransform = new TranslateTransform(destX, destY);

            image.Source = bitmapChain;

            if (interpolate)
            {
                RenderOptions.SetBitmapInterpolationMode(image, BitmapInterpolationMode.LowQuality);
            }
            else
            {
                RenderOptions.SetBitmapInterpolationMode(image,  BitmapInterpolationMode.Default);
            }
        }

        /// <summary>
        /// Sets the clipping rectangle.
        /// </summary>
        /// <param name="clippingRect">The clipping rectangle.</param>
        /// <returns><c>true</c> if the clip rectangle was set.</returns>
        protected override void SetClip(OxyRect clippingRect)
        {
            clip = ToRect(clippingRect);
        }

        /// <summary>
        /// Resets the clip rectangle.
        /// </summary>
        protected override void ResetClip()
        {
            clip = null;
        }

        /// <summary>
        /// Cleans up resources not in use.
        /// </summary>
        /// <remarks>This method is called at the end of each rendering.</remarks>
        public override void CleanUp()
        {
            // Find the images in the cache that has not been used since last call to this method
            // Remove the images from the cache
            foreach (var i in imageCache.Keys.Where(i => !imagesInUse.Contains(i)).ToList())
            {
                imageCache.Remove(i);
            }

            imagesInUse.Clear();
        }

        /// <summary>
        /// Gets the font weight.
        /// </summary>
        /// <param name="fontWeight">The font weight value.</param>
        /// <returns>The font weight.</returns>
        private static FontWeight GetFontWeight(double fontWeight)
        {
            return fontWeight > FontWeights.Normal ? FontWeight.Bold : FontWeight.Normal;
        }

        /// <summary>
        /// Creates an element of the specified type and adds it to the canvas.
        /// </summary>
        /// <typeparam name="T">Type of element to create.</typeparam>
        /// <param name="clipOffsetX">The clip offset executable.</param>
        /// <param name="clipOffsetY">The clip offset asynchronous.</param>
        /// <returns>The element.</returns>
        private T CreateAndAdd<T>(double clipOffsetX = 0, double clipOffsetY = 0) where T : Control, new()
        {
            // TODO: here we can reuse existing elements in the canvas.Children collection
            var element = new T();

            if (clip != null)
            {
                element.Clip = new RectangleGeometry(
                        new Rect(
                            clip.Value.X - clipOffsetX,
                            clip.Value.Y - clipOffsetY,
                            clip.Value.Width,
                            clip.Value.Height));
            }

            canvas.Children.Add(element);

            ApplyToolTip(element);
            return element;
        }

        /// <summary>
        /// Applies the current tool tip to the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        private void ApplyToolTip(Control element)
        {
            if (!string.IsNullOrEmpty(currentToolTip))
            {
                ToolTip.SetTip(element, currentToolTip);
            }
        }

        /// <summary>
        /// Gets the cached brush.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The brush.</returns>
        private IBrush GetCachedBrush(OxyColor color)
        {
            if (color.A == 0)
            {
                return null;
            }

            if (!brushCache.TryGetValue(color, out IBrush brush))
            {
                brush = new SolidColorBrush(color.ToColor());
                brushCache.Add(color, brush);
            }

            return brush;
        }

        /// <summary>
        /// Sets the stroke properties of the specified shape object.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="stroke">The stroke color.</param>
        /// <param name="thickness">The thickness.</param>
        /// <param name="edgeRenderingMode">The edge rendering mode.</param>
        /// <param name="lineJoin">The line join.</param>
        /// <param name="dashArray">The dash array. Use <c>null</c> to get a solid line.</param>
        /// <param name="dashOffset">The dash offset.</param>
        private void SetStroke(
            Shape shape,
            OxyColor stroke,
            double thickness,
            EdgeRenderingMode edgeRenderingMode,
            LineJoin lineJoin = LineJoin.Miter,
            IEnumerable<double> dashArray = null,
            double dashOffset = 0)
        {
            if (thickness > 0 && !stroke.IsUndefined())
            {
                shape.Stroke = GetCachedBrush(stroke);

                switch (lineJoin)
                {
                    case LineJoin.Round:
                        shape.StrokeJoin = PenLineJoin.Round;
                        break;
                    case LineJoin.Bevel:
                        shape.StrokeJoin = PenLineJoin.Bevel;
                        break;

                        // The default StrokeLineJoin is Miter
                }

                if (thickness > 0)
                {
                    shape.StrokeThickness = thickness;
                }

                if (dashArray != null) // what happens if thickness is 0?
                {
                    // ?????? shape.StrokeDashArray = new global::Avalonia.Collections.AvaloniaList<double>(dashArray.Select(dash => dash)); 
                    shape.StrokeDashArray = new global::Avalonia.Collections.AvaloniaList<double>(dashArray);
                    shape.StrokeDashOffset = dashOffset;
                }
            }

            if (edgeRenderingMode == EdgeRenderingMode.PreferSpeed)
            {
                // TODO: use alised lines
            }
        }

        /// <summary>
        /// Gets the bitmap source.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>The bitmap source.</returns>
        private IBitmap GetImageSource(OxyImage image)
        {
            if (image == null)
            {
                return null;
            }

            if (!imagesInUse.Contains(image))
            {
                imagesInUse.Add(image);
            }

            if (imageCache.TryGetValue(image, out var src))
            {
                return src;
            }

            using (var ms = new MemoryStream(image.GetData()))
            {
                var btm = new Bitmap(ms);
                imageCache.Add(image, btm);
                return btm;
            }
        }

        /// <summary>
        /// Draws the line using the MaxPolylinesPerLine and MinPointsPerPolyline properties.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="stroke">The stroke color.</param>
        /// <param name="thickness">The thickness.</param>
        /// <param name="edgeRenderingMode">The edge rendering mode.</param>
        /// <param name="dashArray">The dash array. Use <c>null</c> to get a solid line.</param>
        /// <param name="lineJoin">The line join.</param>
        /// <remarks>See <a href="https://oxyplot.codeplex.com/discussions/456679">discussion</a>.</remarks>
        private void DrawLineBalanced(IList<ScreenPoint> points, OxyColor stroke, double thickness, EdgeRenderingMode edgeRenderingMode, double[] dashArray, LineJoin lineJoin)
        {
            // balance the number of points per polyline and the number of polylines
            var numPointsPerPolyline = Math.Max(points.Count / MaxPolylinesPerLine, MinPointsPerPolyline);

            var polyline = CreateAndAdd<Polyline>();
            SetStroke(polyline, stroke, thickness, edgeRenderingMode, lineJoin, dashArray, 0);
            var pc = new List<Point>(numPointsPerPolyline);

            var n = points.Count;
            double lineLength = 0;
            var dashPatternLength = (dashArray != null) ? dashArray.Sum() : 0;
            var last = new Point();
            for (int i = 0; i < n; i++)
            {
                bool aliased = !this.ShouldUseAntiAliasingForLine(edgeRenderingMode, points);
                var p = aliased ? ToPixelAlignedPoint(points[i]) : ToPoint(points[i]);
                pc.Add(p);

                // alt. 1
                if (dashArray != null)
                {
                    if (i > 0)
                    {
                        var delta = p - last;
                        var dist = Math.Sqrt((delta.X * delta.X) + (delta.Y * delta.Y));
                        lineLength += dist;
                    }

                    last = p;
                }

                // use multiple polylines with limited number of points to improve Avalonia performance
                if (pc.Count >= numPointsPerPolyline)
                {
                    polyline.Points = pc;

                    if (i < n - 1)
                    {
                        // alt.2
                        ////if (dashArray != null)
                        ////{
                        ////    lineLength += this.GetLength(polyline);
                        ////}

                        // start a new polyline at last point so there is no gap (it is not necessary to use the % operator)
                        var dashOffset = dashPatternLength > 0 ? lineLength / thickness : 0;
                        polyline = CreateAndAdd<Polyline>();
                        SetStroke(polyline, stroke, thickness, edgeRenderingMode, lineJoin, dashArray, dashOffset);
                        pc = new List<Point>(numPointsPerPolyline) { pc.Last() };
                    }
                }
            }

            if (pc.Count > 1 || n == 1)
            {
                polyline.Points = pc;
            }
        }

        /// <summary>
        /// Converts a <see cref="ScreenPoint" /> to a <see cref="Point" />.
        /// </summary>
        /// <param name="pt">The screen point.</param>
        /// <returns>A <see cref="Point" />.</returns>
        private static Point ToPoint(ScreenPoint pt)
        {
            return new Point(pt.X, pt.Y);
        }

        /// <summary>
        /// Converts a <see cref="ScreenPoint" /> to a pixel aligned<see cref="Point" />.
        /// </summary>
        /// <param name="pt">The screen point.</param>
        /// <returns>A pixel aligned <see cref="Point" />.</returns>
        private static Point ToPixelAlignedPoint(ScreenPoint pt)
        {
            // adding 0.5 to get pixel boundary alignment, seems to work
            // http://weblogs.asp.net/mschwarz/archive/2008/01/04/silverlight-rectangles-paths-and-line-comparison.aspx
            // http://www.wynapse.com/Silverlight/Tutor/Silverlight_Rectangles_Paths_And_Lines_Comparison.aspx
            // TODO: issue 10221 - should consider line thickness and logical to physical size of pixels
            return new Point(0.5 + (int)pt.X, 0.5 + (int)pt.Y);
        }

        /// <summary>
        /// Converts an <see cref="OxyRect" /> to a <see cref="Rect" />.
        /// </summary>
        /// <param name="r">The rectangle.</param>
        /// <returns>A <see cref="Rect" />.</returns>
        private static Rect ToRect(OxyRect r)
        {
            return new Rect(r.Left, r.Top, r.Width, r.Height);
        }

        /// <summary>
        /// Converts an <see cref="OxyRect" /> to a pixel aligned <see cref="Rect" />.
        /// </summary>
        /// <param name="r">The rectangle.</param>
        /// <returns>A pixel aligned<see cref="Rect" />.</returns>
        private static Rect ToPixelAlignedRect(OxyRect r)
        {
            // TODO: similar changes as in ToPixelAlignedPoint
            var x = 0.5 + (int)r.Left;
            var y = 0.5 + (int)r.Top;
            var ri = 0.5 + (int)r.Right;
            var bo = 0.5 + (int)r.Bottom;
            return new Rect(x, y, ri - x, bo - y);
        }

        /// <summary>
        /// Converts a <see cref="ScreenPoint" /> to a <see cref="Point" />.
        /// </summary>
        /// <param name="pt">The screen point.</param>
        /// <param name="aliased">use pixel alignment conversion if set to <c>true</c>.</param>
        /// <returns>A <see cref="Point" />.</returns>
        private static Point ToPoint(ScreenPoint pt, bool aliased)
        {
            return aliased ? ToPixelAlignedPoint(pt) : ToPoint(pt);
        }

        /// <summary>
        /// Creates a point collection from the specified points.
        /// </summary>
        /// <param name="points">The points to convert.</param>
        /// <param name="aliased">convert to pixel aligned points if set to <c>true</c>.</param>
        /// <returns>The point collection.</returns>
        private static List<Point> ToPointCollection(IEnumerable<ScreenPoint> points, bool aliased)
        {
            return new List<Point>(aliased ? points.Select(ToPixelAlignedPoint) : points.Select(ToPoint));
        }

        /// <summary>
        /// Converts an <see cref="OxyRect"/> to an array of <see cref="ScreenPoint"/>.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>A <see cref="ScreenPoint[]"/>.</returns>
        private static IList<ScreenPoint> RectangleToPolygon(OxyRect rect)
        {
            return new ScreenPoint[]
            {
                rect.BottomLeft,
                rect.TopLeft,
                rect.TopRight,
                rect.BottomRight,
            };
        }
    }
}