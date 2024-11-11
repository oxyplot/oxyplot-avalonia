using Avalonia.Metadata;

[assembly: XmlnsDefinition("http://oxyplot.org/avalonia", "OxyPlot.Avalonia")]
[assembly: XmlnsDefinition("http://oxyplot.org/skiasharp/avalonia", "OxyPlot.SkiaSharp.Avalonia")]
[assembly: XmlnsDefinition("http://oxyplot.org/skiasharp/avalonia/doublebuffered", "OxyPlot.SkiaSharp.Avalonia.DoubleBuffered")]
[assembly: XmlnsDefinition("http://oxyplot.org/skiasharp/avalonia/picturerecorder", "OxyPlot.SkiaSharp.Avalonia.PictureRecorder")]
namespace OxyPlot.SkiaSharp.Avalonia
{
    public static class OxyPlotModule
    {
        public static void EnsureLoaded()
        {
        }
    }
}
