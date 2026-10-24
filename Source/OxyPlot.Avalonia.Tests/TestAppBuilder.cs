using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Simple;
using OxyPlot.Avalonia.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

// The application and the dispatcher are created once per assembly, so a foreign thread cannot occupy the
// dispatcher between the tests. The price of the mode: the tests do not leave application styles and resources behind.
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerAssembly)]

namespace OxyPlot.Avalonia.Tests;

/// <summary>
/// The entry point of the headless Avalonia application for the plot tests.
/// </summary>
public class TestAppBuilder
{
    // Render with Skia instead of the headless stub: otherwise the PNG export writes an empty stream
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApp>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}

/// <summary>
/// The application for the tests: the simple Avalonia theme and the plot styles, like those of the consumers of the package.
/// Without the plot styles neither Plot nor PlotView gets a template and nothing is drawn.
/// </summary>
public class TestApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new SimpleTheme());
        Styles.Add(new StyleInclude(new Uri("avares://OxyPlot.Tests.Avalonia/"))
        {
            Source = new Uri("avares://OxyPlot.Avalonia/Themes/Default.axaml"),
        });
    }
}
