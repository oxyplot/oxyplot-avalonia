using Avalonia.Metadata;

[assembly: XmlnsDefinition("http://oxyplot.org/avalonia", "OxyPlot.Avalonia")]
namespace OxyPlot.Avalonia
{
    using global::Avalonia;
    using global::Avalonia.Styling;

    public static class OxyPlotModule
    {
        public static void Initialize()
        {
            AvaloniaLocator.Current.GetService<IGlobalStyles>().Styles.AddRange(new Themes.Default());
        }
        public static void EnsureLoaded(){}
    }
}
