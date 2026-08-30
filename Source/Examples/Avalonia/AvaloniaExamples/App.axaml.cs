using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using OxyPlot.Avalonia;

namespace AvaloniaExamples
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
#if DEBUG
    this.AttachDeveloperTools();
#endif
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }

    public class Program
    {
        public static AppBuilder BuildAvaloniaApp()
        {
            OxyPlotModule.EnsureLoaded();
            var builder = AppBuilder.Configure<App>()
                .UsePlatformDetect();
#if DEBUG
            builder.LogToTrace();
#endif
            return builder;
        }

        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
    }
}
