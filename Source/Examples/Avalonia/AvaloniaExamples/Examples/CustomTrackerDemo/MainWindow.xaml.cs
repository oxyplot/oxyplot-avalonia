// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Interaction logic for MainWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AvaloniaExamples.Examples.CustomTrackerDemo
{
    using Avalonia.Markup.Xaml;
    using AvaloniaExamples;
    using OxyPlot;
    using OxyPlot.Avalonia.Converters;
    using OxyPlot.Series;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Example("Demonstrates a custom tracker.")]
    public partial class MainWindow : Avalonia.Controls.Window
    {
        public static readonly OxyColorConverter OxyColorConverter = new OxyColorConverter();

        public MainWindow()
        {
            this.InitializeComponent();
            DataContext = new Context();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private class Context
        {
            public PlotModel Model
            {
                get
                {
                    var model = new PlotModel();
                    model.Series.Add(new LineSeries { Title = "Series 1", TrackerKey = "Tracker1", ItemsSource = new List<DataPoint> { new DataPoint(0, 0), new DataPoint(10, 20), new DataPoint(20, 18) } });
                    model.Series.Add(new LineSeries { Title = "Series 2", TrackerKey = "Tracker2", ItemsSource = new List<DataPoint> { new DataPoint(0, 10), new DataPoint(10, 10), new DataPoint(20, 16) } });
                    return model;
                }
            }
        }
    }

    public class NullExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider markupExtensionContext) => null;
    }
}