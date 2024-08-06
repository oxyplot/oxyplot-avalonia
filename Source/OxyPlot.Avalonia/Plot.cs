// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Plot.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Represents a control that displays a <see cref="PlotModel" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Avalonia;

namespace OxyPlot.Avalonia
{
    using global::Avalonia.Controls;
    using global::Avalonia.LogicalTree;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;

    /// <summary>
    /// Represents a control that displays a <see cref="PlotModel" />.
    /// </summary>
    public partial class Plot : PlotView, IPlot
    {
        /// <summary>
        /// The internal model.
        /// </summary>
        private readonly PlotModel internalModel;

        /// <summary>
        /// The default controller.
        /// </summary>
        private readonly IPlotController defaultController;

        /// <summary>
        /// Initializes a new instance of the <see cref="Plot" /> class.
        /// </summary>
        public Plot()
        {
            this.series = [];
            this.axes = [];
            this.annotations = [];
            this.legends = [];

            this.series.CollectionChanged += this.OnSeriesChanged;
            this.axes.CollectionChanged += this.OnAxesChanged;
            this.annotations.CollectionChanged += this.OnAnnotationsChanged;
            this.legends.CollectionChanged += this.OnAnnotationsChanged;

            this.defaultController = new PlotController();
            this.internalModel = new PlotModel();
            ((IPlotModel)this.internalModel).AttachPlotView(this);
        }

        /// <summary>
        /// Gets the annotations.
        /// </summary>
        /// <value>The annotations.</value>
        public ObservableCollection<Annotation> Annotations => this.annotations;

        /// <summary>
        /// Gets the actual model.
        /// </summary>
        /// <value>The actual model.</value>
        public override PlotModel ActualModel => this.internalModel;

        /// <summary>
        /// Gets the actual Plot controller.
        /// </summary>
        /// <value>The actual Plot controller.</value>
        public override IPlotController ActualController => this.defaultController;

        /// <summary>
        /// Updates the model. If Model==<c>null</c>, an internal model will be created. The ActualModel.Update will be called (updates all series data).
        /// </summary>
        /// <param name="updateData">if set to <c>true</c> , all data collections will be updated.</param>
        public override void InvalidatePlot(bool updateData = true)
        {
            base.InvalidatePlot(updateData);
            this.SynchronizeProperties();
            this.SynchronizeSeries();
            this.SynchronizeAxes();
            this.SynchronizeAnnotations();
            this.SynchronizeLegends();
        }

        void IPlot.ElementAppearanceChanged(object element)
        {
            // TODO: determine type of element to perform a more fine-grained update
            base.InvalidatePlot(false);
        }

        void IPlot.ElementDataChanged(object element)
        {
            // TODO: determine type of element to perform a more fine-grained update
            base.InvalidatePlot(true);
        }

        /// <summary>
        /// Called when annotations is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        private void OnAnnotationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.SyncLogicalTree(e);
        }

        /// <summary>
        /// Called when axes is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        private void OnAxesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.SyncLogicalTree(e);
        }

        /// <summary>
        /// Called when series is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        private void OnSeriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.SyncLogicalTree(e);
        }

        /// <summary>
        /// Synchronizes the logical tree.
        /// </summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        private void SyncLogicalTree(NotifyCollectionChangedEventArgs e)
        {
            // In order to get DataContext and binding to work with the series, axes and annotations
            // we add the items to the logical tree
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<ISetLogicalParent>())
                {
                    item.SetParent(this);
                }

                this.LogicalChildren.AddRange(e.NewItems.OfType<ILogical>());
                this.VisualChildren.AddRange(e.NewItems.OfType<Visual>());
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<ISetLogicalParent>())
                {
                    item.SetParent(null);
                }

                foreach (var item in e.OldItems)
                {
                    this.LogicalChildren.Remove((ILogical)item);
                    this.VisualChildren.Remove((Visual)item);
                }
            }

            this.OnAppearanceChanged();
        }

        /// <summary>
        /// Synchronize properties in the internal Plot model
        /// </summary>
        private void SynchronizeProperties()
        {
            var m = this.internalModel;

            m.PlotType = this.PlotType;

            m.PlotMargins = this.PlotMargins.ToOxyThickness();
            m.Padding = this.Padding.ToOxyThickness();
            m.TitlePadding = this.TitlePadding;

            m.Culture = this.Culture;

            m.DefaultColors = this.DefaultColors.Select(c => c.ToOxyColor()).ToArray();
            m.DefaultFont = this.DefaultFont;
            m.DefaultFontSize = this.DefaultFontSize;

            m.Title = this.Title;
            m.TitleColor = this.TitleColor.ToOxyColor();
            m.TitleFont = this.TitleFont;
            m.TitleFontSize = this.TitleFontSize;
            m.TitleFontWeight = (int)this.TitleFontWeight;
            m.TitleToolTip = this.TitleToolTip;

            m.Subtitle = this.Subtitle;
            m.SubtitleColor = this.SubtitleColor.ToOxyColor();
            m.SubtitleFont = this.SubtitleFont;
            m.SubtitleFontSize = this.SubtitleFontSize;
            m.SubtitleFontWeight = (int)this.SubtitleFontWeight;

            m.TextColor = this.TextColor.ToOxyColor();
            m.SelectionColor = this.SelectionColor.ToOxyColor();

            m.RenderingDecorator = this.RenderingDecorator;

            m.AxisTierDistance = this.AxisTierDistance;

            m.IsLegendVisible = this.IsLegendVisible;

            m.PlotAreaBackground = this.PlotAreaBackground.ToOxyColor();
            m.PlotAreaBorderColor = this.PlotAreaBorderColor.ToOxyColor();
            m.PlotAreaBorderThickness = this.PlotAreaBorderThickness.ToOxyThickness();
        }

        /// <summary>
        /// Synchronizes the annotations in the internal model.
        /// </summary>
        private void SynchronizeAnnotations()
        {
            this.internalModel.Annotations.Clear();
            foreach (var a in this.Annotations)
            {
                this.internalModel.Annotations.Add(a.CreateModel());
            }
        }

        /// <summary>
        /// Synchronizes the axes in the internal model.
        /// </summary>
        private void SynchronizeAxes()
        {
            this.internalModel.Axes.Clear();
            foreach (var a in this.Axes)
            {
                this.internalModel.Axes.Add(a.CreateModel());
            }
        }

        /// <summary>
        /// Synchronizes the series in the internal model.
        /// </summary>
        private void SynchronizeSeries()
        {
            this.internalModel.Series.Clear();
            foreach (var s in this.Series)
            {
                this.internalModel.Series.Add(s.CreateModel());
            }
        }

        /// <summary>
        /// Synchronizes the legends in the internal model.
        /// </summary>
        private void SynchronizeLegends()
        {
            this.internalModel.Legends.Clear();
            foreach (var l in this.Legends)
            {
                this.internalModel.Legends.Add(l.CreateModel());
            }
        }
    }
}
