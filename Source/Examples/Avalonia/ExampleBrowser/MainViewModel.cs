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
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Represents the view-model for the main window.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private ExampleInfo example;
        private Renderer selectedRenderer;
        private PlotModel model;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        public MainViewModel()
        {
            this.model = new PlotModel() { Title = "Example Browser", Subtitle = "Select an example from the list" };
            this.Categories = Examples.GetList()
                .GroupBy(e => e.Category)
                .Select(g => new Category(g.Key, g.ToList()))
                .OrderBy(c => c.Key)
                .ToList();
        }

        public IReadOnlyList<Renderer> Renderers { get; } = Enum.GetValues<Renderer>();

        public Renderer SelectedRenderer
        {
            get => this.selectedRenderer;
            set
            {
                if (this.selectedRenderer != value)
                {
                    this.selectedRenderer = value;
                    this.CoerceSelectedRenderer();
                    this.OnPropertyChanged(nameof(this.SelectedRenderer));
                }
            }
        }

        public List<Category> Categories { get; }

        /// <summary>
        /// Gets the plot model.
        /// </summary>
        public ExampleInfo Example
        {
            get => this.example;
            set
            {
                if (value == null)
                {
                    // many listboxes are bound to this, so ignore null assignments and hope that doesn't break anything
                    return;
                }

                if (this.example != value)
                {
                    this.example = value;
                    this.OnPropertyChanged(nameof(this.Example));
                    this.CoerceExample();
                }
            }
        }

        public PlotModel CanvasModel => this.SelectedRenderer == Renderer.Canvas ? this.model : null;

        public PlotModel SkiaSharpModel => this.SelectedRenderer == Renderer.SkiaSharp ? this.model : null;
        public PlotModel SkiaSharpDoubleBufferedModel => this.SelectedRenderer == Renderer.SkiaSharpDoubleBuffered ? this.model : null;

        public void ChangeExample(ExampleInfo example)
        {
            this.Example = example;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void CoerceSelectedRenderer()
        {
            ((IPlotModel)this.model)?.AttachPlotView(null);
            this.OnPropertyChanged(nameof(this.CanvasModel));
            this.OnPropertyChanged(nameof(this.SkiaSharpModel));
            this.OnPropertyChanged(nameof(this.SkiaSharpDoubleBufferedModel));
        }

        private void CoerceExample()
        {
            this.model = this.example?.PlotModel;
            this.model?.InvalidatePlot(true);
            this.OnPropertyChanged(nameof(this.CanvasModel));
            this.OnPropertyChanged(nameof(this.SkiaSharpModel));
            this.OnPropertyChanged(nameof(this.SkiaSharpDoubleBufferedModel));
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}