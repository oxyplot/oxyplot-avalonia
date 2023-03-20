using Avalonia.Metadata;
using Avalonia.Platform;

[assembly: XmlnsDefinition("http://oxyplot.org/avalonia", "OxyPlot.Avalonia")]
namespace OxyPlot.Avalonia
{
    using global::Avalonia;
    using global::Avalonia.Styling;

    public static class OxyPlotModule
    {
        public static void EnsureLoaded()
        {

        }
    }
}
