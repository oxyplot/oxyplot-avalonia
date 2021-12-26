// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Represents the view-model for the main window.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExampleBrowser
{
    using ExampleLibrary;
    using OxyPlot;
    using OxyPlot.Series;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    public class Category
    {
        public Category(string key, List<ExampleInfo> examples)
        {
            Key = key;
            Examples = examples ?? throw new ArgumentNullException(nameof(examples));
        }

        public string Key { get; }
        public List<ExampleInfo> Examples { get; }
    }

    /// <summary>
    /// Represents the view-model for the main window.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        public MainViewModel()
        {
            Model = new PlotModel() { Title = "Example Browser", Subtitle = "Select an example from the list" };
            Categories = Examples.GetList()
                .GroupBy(e => e.Category)
                .Select(g => new Category(g.Key, g.ToList()))
                .OrderBy(c => c.Key)
                .ToList();
        }

        public List<Category> Categories { get; }

        private ExampleInfo example;
        /// <summary>
        /// Gets the plot model.
        /// </summary>
        public ExampleInfo Example
        {
            get => example;
            set
            {
                if (value == null)
                {
                    // many listboxes are bound to this, so ignore null assignments and hope that doesn't break anything
                    return;
                }

                if (example != value)
                {
                    example = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Example)));
                    Model = example?.PlotModel;
                    Model?.InvalidatePlot(true);
                }
            }
        }

        private PlotModel model;
        /// <summary>
        /// Gets the plot model.
        /// </summary>
        public PlotModel Model
        {
            get => model;
            set
            {
                if (model != value)
                {
                    model = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Model)));
                }
            }
        }

        public void ChangeExample(ExampleInfo example)
        {
            Example = example;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}