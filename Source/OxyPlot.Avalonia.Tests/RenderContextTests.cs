using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using OxyPlot;
using OxyPlot.Avalonia;
using Image = Avalonia.Controls.Image;

namespace OxyPlot.Avalonia.Tests;

/// <summary>
/// Tests of the render context on an Avalonia canvas.
/// </summary>
public class RenderContextTests
{
    [AvaloniaFact]
    public void MeasureText_LongText_IsNotLimitedTo1000()
    {
        var context = new CanvasRenderContext(new Canvas());

        var size = context.MeasureText(new string('W', 500), "Arial", 20, 400);

        // with the 1000x1000 limit the width came out as exactly 1000
        Assert.True(size.Width > 1000);
    }

    [AvaloniaFact]
    public void DrawText_LongCenteredText_IsShiftedByHalfOfFullWidth()
    {
        var canvas = new Canvas();
        var context = new CanvasRenderContext(canvas);

        context.DrawText(new ScreenPoint(0, 0), new string('W', 500), OxyColors.Black, "Arial", 20, 400, 0, HorizontalAlignment.Center, VerticalAlignment.Top, null);

        var text = Assert.IsType<TextBlock>(Assert.Single(canvas.Children));
        Assert.True(text.RenderTransform!.Value.M31 < -1000);
    }

    [AvaloniaFact]
    public void DrawImage_SourcePart_CropsSourceAndKeepsClip()
    {
        var canvas = new Canvas();
        var context = new CanvasRenderContext(canvas);
        context.PushClip(new OxyRect(0, 0, 50, 50));

        context.DrawImage(CreateImage(), 1, 1, 2, 2, 10, 10, 20, 20, 1, false);

        var image = Assert.IsType<Image>(Assert.Single(canvas.Children));
        var cropped = Assert.IsType<CroppedBitmap>(image.Source);
        Assert.Equal(new PixelRect(1, 1, 2, 2), cropped.SourceRect);

        // the clipping to the plot area is translated to the coordinates of the element and is not overridden by the source area
        var clip = Assert.IsType<RectangleGeometry>(image.Clip);
        Assert.Equal(new Rect(-10, -10, 50, 50), clip.Rect);
    }

    [AvaloniaFact]
    public void DrawImage_WholeSource_UsesBitmapAsIs()
    {
        var canvas = new Canvas();
        var context = new CanvasRenderContext(canvas);

        context.DrawImage(CreateImage(), 0, 0, 4, 4, 10, 10, 20, 20, 1, false);

        var image = Assert.IsType<Image>(Assert.Single(canvas.Children));
        Assert.IsAssignableFrom<Bitmap>(image.Source);
    }

    [AvaloniaFact]
    public void DrawShapes_NonFiniteGeometry_IsSkipped()
    {
        var canvas = new Canvas();
        var context = new CanvasRenderContext(canvas);

        context.DrawRectangle(new OxyRect(double.NegativeInfinity, 0, double.PositiveInfinity, 10), OxyColors.Red, OxyColors.Black, 1, EdgeRenderingMode.Automatic);
        context.DrawEllipse(new OxyRect(0, 0, double.NaN, double.NaN), OxyColors.Red, OxyColors.Black, 1, EdgeRenderingMode.Automatic);
        context.DrawPolygon(new[] { new ScreenPoint(0, 0), new ScreenPoint(double.NaN, 1), new ScreenPoint(1, 1) }, OxyColors.Red, OxyColors.Black, 1, EdgeRenderingMode.Automatic, null, LineJoin.Miter);
        context.DrawLineSegments(new[] { new ScreenPoint(0, 0), new ScreenPoint(double.NaN, 1) }, OxyColors.Black, 1, EdgeRenderingMode.Automatic, null, LineJoin.Miter);
        context.DrawText(new ScreenPoint(double.NaN, 0), "x", OxyColors.Black, null, 12, 400, 0, HorizontalAlignment.Left, VerticalAlignment.Top, null);

        // a shape with NaN or infinity would break the layout pass of the canvas
        Assert.Empty(canvas.Children);
    }

    [AvaloniaFact]
    public void DrawLine_NonFinitePoint_IsDroppedFromLine()
    {
        var canvas = new Canvas();
        var context = new CanvasRenderContext(canvas);

        context.DrawLine(new[] { new ScreenPoint(0, 0), new ScreenPoint(double.NaN, double.NaN), new ScreenPoint(10, 10) }, OxyColors.Black, 1, EdgeRenderingMode.Automatic, null, LineJoin.Miter);
        var window = Ui.Show(canvas);

        var line = Assert.IsType<Polyline>(Assert.Single(canvas.Children));
        Assert.Equal(2, line.Points.Count);
        window.Close();
    }

    /// <summary>
    /// A 4x4 picture in the PNG format.
    /// </summary>
    private static OxyImage CreateImage()
    {
        var pixels = new OxyColor[4, 4];
        for (var x = 0; x < 4; x++)
        {
            for (var y = 0; y < 4; y++)
            {
                pixels[x, y] = (x + y) % 2 == 0 ? OxyColors.Black : OxyColors.White;
            }
        }

        return OxyImage.Create(pixels, ImageFormat.Png);
    }
}
