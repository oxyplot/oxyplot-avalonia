// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Interaction logic for MainWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using OxyPlot;
using System.Collections.ObjectModel;

namespace AvaloniaExamples.Examples.BarSeriesDemo
{
    using AvaloniaExamples;
    using OxyPlot.Axes;
    using OxyPlot.Legends;
    using OxyPlot.Series;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Example("Shows bar series.")]
    public partial class MainWindow : Avalonia.Controls.Window
    {
        public MainWindow()
        {
            // Create some data
            var items = new Collection<Item>
            {
                new Item { Label = "Apples", Value1 = 37, Value2 = 12, Value3 = 19, Value4 = 42},
                new Item { Label = "Pears", Value1 = 7, Value2 = 21, Value3 = 9, Value4 = 21},
                new Item { Label = "Bananas", Value1 = 23, Value2 = 2, Value3 = 29, Value4 = 10}
            };

            // Create the plot model
            var tmp = new PlotModel { Title = "Bar series" };
            tmp.Legends.Add(new Legend { LegendPlacement = LegendPlacement.Outside, LegendPosition = LegendPosition.RightTop, LegendOrientation = LegendOrientation.Vertical });

            // Add the axes, note that MinimumPadding and AbsoluteMinimum should be set on the value axis.
            tmp.Axes.Add(new CategoryAxis { Position = AxisPosition.Left, ItemsSource = items, LabelField = "Label" });
            tmp.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MinimumPadding = 0, AbsoluteMinimum = 0 });

            // Add the series, note that the BarSeries are using the same ItemsSource as the CategoryAxis.
            tmp.Series.Add(new BarSeries { Title = "2009", ItemsSource = items, ValueField = "Value1" });
            tmp.Series.Add(new BarSeries { Title = "2010", ItemsSource = items, ValueField = "Value2" });
            tmp.Series.Add(new BarSeries { Title = "2011", ItemsSource = items, ValueField = "Value3" });
            tmp.Series.Add(new BarSeries { Title = "2012", ItemsSource = items, ValueField = "Value4" });

            this.DataContext = new { Model1 = tmp, Items = items };

            this.InitializeComponent();

            App.AttachDevTools(this);
        }

        private void InitializeComponent()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }
    }

    public class Item : BarItem
    {
        public string Label { get; set; }
        public double Value1 { get; set; }
        public double Value2 { get; set; }
        public double Value3 { get; set; }
        public double Value4 { get; set; }
    }
}