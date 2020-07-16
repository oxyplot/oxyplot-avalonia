using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using OxyPlot.Avalonia;

namespace MemoryTest
{
    class App : Application
    {

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            base.Initialize();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (!(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop))
            {
                throw new PlatformNotSupportedException();
            }

            desktop.MainWindow = new MainWindow();

            base.OnFrameworkInitializationCompleted();
        }

        static void Main(string[] args)
        {
            OxyPlotModule.EnsureLoaded();
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
#if DEBUG
                .LogToDebug()
#endif
                .StartWithClassicDesktopLifetime(args);
        }

        public static void AttachDevTools(Window window)
        {
#if DEBUG
			DevToolsExtensions.AttachDevTools(window);
#endif
        }
    }
}
