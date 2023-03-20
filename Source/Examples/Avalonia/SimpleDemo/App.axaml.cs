using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using OxyPlot.Avalonia;
using System;

namespace SimpleDemo
{
    class App : Application
    {
        public App()
        {
            RegisterServices();
        }

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

        public static void Main(string[] args)
        {
            OxyPlotModule.EnsureLoaded();
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .StartWithClassicDesktopLifetime(args);
        }
    }
}
