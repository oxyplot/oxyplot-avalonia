using Avalonia.Metadata;

[assembly: XmlnsDefinition("http://oxyplot.org/avalonia", "OxyPlot.Avalonia")]
[assembly: XmlnsDefinition("http://oxyplot.org/skiasharp/avalonia", "OxyPlot.SkiaSharp.Avalonia")]
[assembly: XmlnsDefinition("http://oxyplot.org/skiasharp/avalonia/doublebuffered", "OxyPlot.SkiaSharp.Avalonia.DoubleBuffered")]
namespace OxyPlot.SkiaSharp.Avalonia
{
    public static class OxyPlotModule
    {
        public static void EnsureLoaded()
        {
        }
    }
}
