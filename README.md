[![nuget](https://img.shields.io/nuget/v/OxyPlot.Avalonia.svg)](https://www.nuget.org/packages/OxyPlot.Avalonia) ![License](https://img.shields.io/github/license/oxyplot/oxyplot-avalonia.svg) ![Size](https://img.shields.io/github/repo-size/oxyplot/oxyplot-avalonia.svg)

# OxyPlot.Avalonia

[OxyPlot](https://github.com/oxyplot) is a plotting library for .NET. This [package](https://www.nuget.org/packages/OxyPlot.Avalonia) targets Avalonia applications.

```
dotnet add package OxyPlot.Avalonia
```

### Usage

To use the library, add the following to your `App.xaml`:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="Sensei.Presentation.Avalonia.App">
    <Application.Styles>
        <StyleInclude Source="avares://Avalonia.Themes.Default/DefaultTheme.xaml"/>
        <StyleInclude Source="avares://Avalonia.Themes.Default/Accents/BaseLight.xaml"/>
      
        <!-- Add the line below to get OxyPlot UI theme applied. -->
        <StyleInclude Source="resm:OxyPlot.Avalonia.Themes.Default.xaml?assembly=OxyPlot.Avalonia"/>
    </Application.Styles>
</Application>
```

Then, you can add plots to your application, as such:

```xml
<avalonia:Plot Height="150" 
               PlotMargins="50 0 0 0"
               PlotAreaBorderColor="#999999">
    <avalonia:Plot.Series>
        <avalonia:AreaSeries 
            DataFieldX="Index"
            DataFieldY="Value"
            Items="{Binding Path=Values}"
            Color="#fd6d00" />
    </avalonia:Plot.Series>
</avalonia:Plot>
```

See the [AvaloniaExamples](https://github.com/oxyplot/oxyplot-avalonia/tree/master/Source/Examples/Avalonia/AvaloniaExamples) project and [OxyPlot Documentation](https://readthedocs.org/projects/oxyplot/downloads/pdf/latest/) to learn how to create more complex plots. 


### Installing Preview Versions

To access the latest version of `OxyPlot.Avalonia` you can add this repo as a [submodule](https://git-scm.com/book/en/v2/Git-Tools-Submodules) to your own git repo:

```sh
mkdir ./external
git submodule add git@github.com:oxyplot/oxyplot-avalonia.git ./external/oxyplot-avalonia
# Reference the ../external/oxyplot-avalonia/Source/OxyPlot.Avalonia/OxyPlot.Avalonia.csproj project then.
```

Another way is to import our [Azure Artifacts NuGet package feed](https://worldbeater.visualstudio.com/OxyPlot.Avalonia/_packaging) by creating the following `nuget.config` file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear /> <!-- Add other external NuGet package sources here -->
    <add key="OxyPlot.Avalonia-CI" value="https://worldbeater.pkgs.visualstudio.com/OxyPlot.Avalonia/_packaging/OxyPlot.Avalonia-CI/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

Next, install the latest preview version of the `OxyPlot.Avalonia` package as such:

```
dotnet add package OxyPlot.Avalonia
```
