// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Legend.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Represents a control that displays a <see cref="OxyPlot.Legends.Legend" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Avalonia;

namespace OxyPlot.Avalonia
{
    using global::Avalonia.Controls;

    /// <summary>
    /// Represents a control that displays a <see cref="Legends.Legend" />.
    /// </summary>
    public partial class Legend : Control
    {
        /// <summary>
        /// The internal model.
        /// </summary>
        private readonly Legends.Legend InternalLegend;

        /// <summary>
        /// Initializes static members of the <see cref="Legend" /> class.
        /// </summary>
        public Legend()
        {
            this.InternalLegend = new Legends.Legend();
        }

        /// <summary>
        /// Synchronize properties in the internal Plot model
        /// </summary>
        private void SynchronizeProperties()
        {
            var m = this.InternalLegend;

            m.LegendTextColor = this.LegendTextColor.ToOxyColor();
            m.LegendTitle = this.LegendTitle;
            m.LegendTitleColor = this.LegendTitleColor.ToOxyColor();
            m.LegendTitleFont = this.LegendTitleFont;
            m.LegendTitleFontSize = this.LegendTitleFontSize;
            m.LegendTitleFontWeight = (int)this.LegendTitleFontWeight;
            m.LegendFont = this.LegendFont;
            m.LegendFontSize = this.LegendFontSize;
            m.LegendFontWeight = (int)this.LegendFontWeight;
            m.LegendSymbolLength = this.LegendSymbolLength;
            m.LegendSymbolMargin = this.LegendSymbolMargin;
            m.LegendPadding = this.LegendPadding;
            m.LegendColumnSpacing = this.LegendColumnSpacing;
            m.LegendItemSpacing = this.LegendItemSpacing;
            m.LegendLineSpacing = this.LegendLineSpacing;
            m.LegendMargin = this.LegendMargin;
            m.LegendMaxHeight = this.LegendMaxHeight;
            m.LegendMaxWidth = this.LegendMaxWidth;

            m.LegendBackground = this.LegendBackground.ToOxyColor();
            m.LegendBorder = this.LegendBorder.ToOxyColor();
            m.LegendBorderThickness = this.LegendBorderThickness;

            m.LegendPlacement = this.LegendPlacement;
            m.LegendPosition = this.LegendPosition;
            m.LegendOrientation = this.LegendOrientation;
            m.LegendItemOrder = this.LegendItemOrder;
            m.LegendItemAlignment = this.LegendItemAlignment.ToHorizontalAlignment();
            m.LegendSymbolPlacement = this.LegendSymbolPlacement;

            m.IsLegendVisible = this.IsLegendVisible;
            m.EdgeRenderingMode = this.EdgeRenderingMode;
        }

        /// <summary>
        /// Handles changes in appearance.
        /// </summary>
        /// <param name="d">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        protected static void AppearanceChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((Legend)d).OnVisualChanged();
        }

        /// <summary>
        /// The on visual changed handler.
        /// </summary>
        protected void OnVisualChanged()
        {
            (this.Parent as IPlot)?.ElementAppearanceChanged(this);
        }

        /// <summary>
        /// Handles changes in data.
        /// </summary>
        /// <param name="d">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        protected static void DataChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((Legend)d).OnDataChanged();
        }

        /// <summary>
        /// The on data changed handler.
        /// </summary>
        protected void OnDataChanged()
        {
            (this.Parent as IPlot)?.ElementDataChanged(this);
        }

        static Legend()
        {
            IsLegendVisibleProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);

            LegendTextColorProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendTitleProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendTitleColorProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendTitleFontProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendTitleFontSizeProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendTitleFontWeightProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendFontProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendFontSizeProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendFontWeightProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendSymbolLengthProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendSymbolMarginProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendPaddingProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendColumnSpacingProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendItemSpacingProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendLineSpacingProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendMarginProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendMaxHeightProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendMaxWidthProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);

            LegendBackgroundProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendBorderProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendBorderThicknessProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);

            LegendPlacementProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendPositionProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendOrientationProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendItemOrderProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendItemAlignmentProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            LegendSymbolPlacementProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);

            IsLegendVisibleProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
            EdgeRenderingModeProperty.Changed.AddClassHandler<Legend>(AppearanceChanged);
        }

        /// <summary>
        /// Creates the internal legend.
        /// </summary>
        /// <returns>The internal legend.</returns>
        public Legends.Legend CreateModel()
        {
            this.SynchronizeProperties();
            return this.InternalLegend;
        }
    }
}
