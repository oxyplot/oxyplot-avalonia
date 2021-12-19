// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Legend.Properties.cs" company="OxyPlot">
//   Copyright (c) 2020 OxyPlot contributors
// </copyright>
// <summary>
//   Represents a control that displays a <see cref="OxyPlot.Legends.Legend" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Avalonia;

namespace OxyPlot.Avalonia
{
    using global::Avalonia.Controls;
    using global::Avalonia.Media;
    using OxyPlot.Legends;

    /// <summary>
    /// Represents a control that displays a <see cref="OxyPlot.Legends.Legend" />.
    /// </summary>
    /// <remarks>This file contains dependency properties used for defining the Plot in XAML. These properties are only used when Model is <c>null</c>. In this case an internal PlotModel is created and the dependency properties are copied from the control to the internal Legend.</remarks>
    public partial class Legend : Control
    {
        /// <summary>
        /// Identifies the <see cref="IsLegendVisible"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<bool> IsLegendVisibleProperty = AvaloniaProperty.Register<Legend, bool>(nameof(IsLegendVisible), true);

        /// <summary>
        /// Identifies the <see cref="LegendBackground"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<Color> LegendBackgroundProperty = AvaloniaProperty.Register<Legend, Color>(nameof(LegendBackground), MoreColors.Undefined);

        /// <summary>
        /// Identifies the <see cref="LegendBorder"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<Color> LegendBorderProperty = AvaloniaProperty.Register<Legend, Color>(nameof(LegendBorder), MoreColors.Undefined);

        /// <summary>
        /// Identifies the <see cref="LegendBorderThickness"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<double> LegendBorderThicknessProperty = AvaloniaProperty.Register<Legend, double>(nameof(LegendBorderThickness), 1.0);

        /// <summary>
        /// Identifies the <see cref="LegendFont"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<string> LegendFontProperty = AvaloniaProperty.Register<Legend, string>(nameof(LegendFont), null);

        /// <summary>
        /// Identifies the <see cref="LegendFontSize"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<double> LegendFontSizeProperty = AvaloniaProperty.Register<Legend, double>(nameof(LegendFontSize), 12.0);

        /// <summary>
        /// Identifies the <see cref="LegendFontWeight"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<FontWeight> LegendFontWeightProperty = AvaloniaProperty.Register<Legend, FontWeight>(nameof(LegendFontWeight), FontWeight.Normal);

        /// <summary>
        /// Identifies the <see cref="LegendItemAlignment"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<global::Avalonia.Layout.HorizontalAlignment> LegendItemAlignmentProperty = AvaloniaProperty.Register<Legend, global::Avalonia.Layout.HorizontalAlignment>(nameof(LegendItemAlignment), global::Avalonia.Layout.HorizontalAlignment.Left);

        /// <summary>
        /// Identifies the <see cref="LegendItemOrder"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<LegendItemOrder> LegendItemOrderProperty = AvaloniaProperty.Register<Legend, LegendItemOrder>(nameof(LegendItemOrder), LegendItemOrder.Normal);

        /// <summary>
        /// Identifies the <see cref="LegendItemSpacing"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<double> LegendItemSpacingProperty = AvaloniaProperty.Register<Legend, double>(nameof(LegendItemSpacing), 24.0);

        /// <summary>
        /// Identifies the <see cref="LegendLineSpacing"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<double> LegendLineSpacingProperty = AvaloniaProperty.Register<Legend, double>(nameof(LegendLineSpacing), 0d);

        /// <summary>
        /// Identifies the <see cref="LegendMargin"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<double> LegendMarginProperty = AvaloniaProperty.Register<Legend, double>(nameof(LegendMargin), 8.0);

        /// <summary>
        /// Identifies the <see cref="LegendMaxHeight"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<double> LegendMaxHeightProperty = AvaloniaProperty.Register<Legend, double>(nameof(LegendMaxHeight), double.NaN);

        /// <summary>
        /// Identifies the <see cref="LegendMaxWidth"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<double> LegendMaxWidthProperty = AvaloniaProperty.Register<Legend, double>(nameof(LegendMaxWidth), double.NaN);

        /// <summary>
        /// Identifies the <see cref="LegendOrientation"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<LegendOrientation> LegendOrientationProperty = AvaloniaProperty.Register<Legend, LegendOrientation>(nameof(LegendOrientation), LegendOrientation.Vertical);

        /// <summary>
        /// Identifies the <see cref="LegendColumnSpacing"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<double> LegendColumnSpacingProperty = AvaloniaProperty.Register<Legend, double>(nameof(LegendColumnSpacing), 8.0);

        /// <summary>
        /// Identifies the <see cref="LegendPadding"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<double> LegendPaddingProperty = AvaloniaProperty.Register<Legend, double>(nameof(LegendPadding), 8.0);

        /// <summary>
        /// Identifies the <see cref="LegendPlacement"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<LegendPlacement> LegendPlacementProperty = AvaloniaProperty.Register<Legend, LegendPlacement>(nameof(LegendPlacement), LegendPlacement.Inside);

        /// <summary>
        /// Identifies the <see cref="LegendPosition"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<LegendPosition> LegendPositionProperty = AvaloniaProperty.Register<Legend, LegendPosition>(nameof(LegendPosition), LegendPosition.RightTop);

        /// <summary>
        /// Identifies the <see cref="LegendSymbolLength"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<double> LegendSymbolLengthProperty = AvaloniaProperty.Register<Legend, double>(nameof(LegendSymbolLength), 16.0);

        /// <summary>
        /// Identifies the <see cref="LegendSymbolMargin"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<double> LegendSymbolMarginProperty = AvaloniaProperty.Register<Legend, double>(nameof(LegendSymbolMargin), 4.0);

        /// <summary>
        /// Identifies the <see cref="LegendSymbolPlacement"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<LegendSymbolPlacement> LegendSymbolPlacementProperty = AvaloniaProperty.Register<Legend, LegendSymbolPlacement>(nameof(LegendSymbolPlacement), LegendSymbolPlacement.Left);

        /// <summary>
        /// Identifies the <see cref="LegendTextColor"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<Color> LegendTextColorProperty = AvaloniaProperty.Register<Legend, Color>(nameof(LegendTextColor), MoreColors.Automatic);

        /// <summary>
        /// Identifies the <see cref="LegendTitle"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<string> LegendTitleProperty = AvaloniaProperty.Register<Legend, string>(nameof(LegendTitle), null);

        /// <summary>
        /// Identifies the <see cref="LegendTextColor"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<Color> LegendTitleColorProperty = AvaloniaProperty.Register<Legend, Color>(nameof(LegendTitleColor), MoreColors.Automatic);

        /// <summary>
        /// Identifies the <see cref="LegendTitleFont"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<string> LegendTitleFontProperty = AvaloniaProperty.Register<Legend, string>(nameof(LegendTitleFont), null);

        /// <summary>
        /// Identifies the <see cref="LegendTitleFontSize"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<double> LegendTitleFontSizeProperty = AvaloniaProperty.Register<Legend, double>(nameof(LegendTitleFontSize), 12.0);

        /// <summary>
        /// Identifies the <see cref="LegendTitleFontWeight"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<FontWeight> LegendTitleFontWeightProperty = AvaloniaProperty.Register<Legend, FontWeight>(nameof(LegendTitleFontWeight), FontWeight.Bold);

        /// <summary>
        /// Identifies the <see cref="EdgeRenderingMode"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<EdgeRenderingMode> EdgeRenderingModeProperty = AvaloniaProperty.Register<Legend, EdgeRenderingMode>(nameof(EdgeRenderingMode), EdgeRenderingMode.Automatic);

        /// <summary>
        /// Gets or sets LegendBackground.
        /// </summary>
        public Color LegendBackground
        {
            get
            {
                return GetValue(LegendBackgroundProperty);
            }

            set
            {
                SetValue(LegendBackgroundProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets LegendBorder.
        /// </summary>
        public Color LegendBorder
        {
            get
            {
                return GetValue(LegendBorderProperty);
            }

            set
            {
                SetValue(LegendBorderProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets LegendBorderThickness.
        /// </summary>
        public double LegendBorderThickness
        {
            get
            {
                return GetValue(LegendBorderThicknessProperty);
            }

            set
            {
                SetValue(LegendBorderThicknessProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the spacing between columns of legend items (only for vertical orientation).
        /// </summary>
        /// <value>The spacing in device independent units.</value>
        public double LegendColumnSpacing
        {
            get
            {
                return GetValue(LegendColumnSpacingProperty);
            }

            set
            {
                SetValue(LegendColumnSpacingProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets LegendFont.
        /// </summary>
        public string LegendFont
        {
            get
            {
                return GetValue(LegendFontProperty);
            }

            set
            {
                SetValue(LegendFontProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets LegendFontSize.
        /// </summary>
        public double LegendFontSize
        {
            get
            {
                return GetValue(LegendFontSizeProperty);
            }

            set
            {
                SetValue(LegendFontSizeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets LegendFontWeight.
        /// </summary>
        public FontWeight LegendFontWeight
        {
            get
            {
                return GetValue(LegendFontWeightProperty);
            }

            set
            {
                SetValue(LegendFontWeightProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets LegendItemAlignment.
        /// </summary>
        public global::Avalonia.Layout.HorizontalAlignment LegendItemAlignment
        {
            get
            {
                return GetValue(LegendItemAlignmentProperty);
            }

            set
            {
                SetValue(LegendItemAlignmentProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets LegendItemOrder.
        /// </summary>
        public LegendItemOrder LegendItemOrder
        {
            get
            {
                return GetValue(LegendItemOrderProperty);
            }

            set
            {
                SetValue(LegendItemOrderProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the horizontal spacing between legend items when the orientation is horizontal.
        /// </summary>
        /// <value>The horizontal distance between items in device independent units.</value>
        public double LegendItemSpacing
        {
            get
            {
                return GetValue(LegendItemSpacingProperty);
            }

            set
            {
                SetValue(LegendItemSpacingProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the vertical spacing between legend items.
        /// </summary>
        /// <value>The spacing in device independent units.</value>
        public double LegendLineSpacing
        {
            get
            {
                return GetValue(LegendLineSpacingProperty);
            }

            set
            {
                SetValue(LegendLineSpacingProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the max height of the legend.
        /// </summary>
        /// <value>The max width of the legends.</value>
        public double LegendMaxHeight
        {
            get
            {
                return GetValue(LegendMaxHeightProperty);
            }

            set
            {
                SetValue(LegendMaxHeightProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the max width of the legend.
        /// </summary>
        /// <value>The max width of the legends.</value>
        public double LegendMaxWidth
        {
            get
            {
                return GetValue(LegendMaxWidthProperty);
            }

            set
            {
                SetValue(LegendMaxWidthProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets LegendMargin.
        /// </summary>
        public double LegendMargin
        {
            get
            {
                return GetValue(LegendMarginProperty);
            }

            set
            {
                SetValue(LegendMarginProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets LegendOrientation.
        /// </summary>
        public LegendOrientation LegendOrientation
        {
            get
            {
                return GetValue(LegendOrientationProperty);
            }

            set
            {
                SetValue(LegendOrientationProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the legend padding.
        /// </summary>
        public double LegendPadding
        {
            get
            {
                return GetValue(LegendPaddingProperty);
            }

            set
            {
                SetValue(LegendPaddingProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets LegendPlacement.
        /// </summary>
        public LegendPlacement LegendPlacement
        {
            get
            {
                return GetValue(LegendPlacementProperty);
            }

            set
            {
                SetValue(LegendPlacementProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the legend position.
        /// </summary>
        /// <value>The legend position.</value>
        public LegendPosition LegendPosition
        {
            get
            {
                return GetValue(LegendPositionProperty);
            }

            set
            {
                SetValue(LegendPositionProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets LegendSymbolLength.
        /// </summary>
        public double LegendSymbolLength
        {
            get
            {
                return GetValue(LegendSymbolLengthProperty);
            }

            set
            {
                SetValue(LegendSymbolLengthProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets LegendSymbolMargin.
        /// </summary>
        public double LegendSymbolMargin
        {
            get
            {
                return GetValue(LegendSymbolMarginProperty);
            }

            set
            {
                SetValue(LegendSymbolMarginProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets LegendSymbolPlacement.
        /// </summary>
        public LegendSymbolPlacement LegendSymbolPlacement
        {
            get
            {
                return GetValue(LegendSymbolPlacementProperty);
            }

            set
            {
                SetValue(LegendSymbolPlacementProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets LegendTitleFont.
        /// </summary>
        public string LegendTitleFont
        {
            get
            {
                return GetValue(LegendTitleFontProperty);
            }

            set
            {
                SetValue(LegendTitleFontProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the text color of the legends.
        /// </summary>
        public Color LegendTextColor
        {
            get
            {
                return GetValue(LegendTextColorProperty);
            }

            set
            {
                SetValue(LegendTextColorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the legend title.
        /// </summary>
        public string LegendTitle
        {
            get
            {
                return GetValue(LegendTitleProperty);
            }

            set
            {
                SetValue(LegendTitleProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets color of the legend title.
        /// </summary>
        public Color LegendTitleColor
        {
            get
            {
                return GetValue(LegendTitleColorProperty);
            }

            set
            {
                SetValue(LegendTitleColorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the font size of the legend titles.
        /// </summary>
        public double LegendTitleFontSize
        {
            get
            {
                return GetValue(LegendTitleFontSizeProperty);
            }

            set
            {
                SetValue(LegendTitleFontSizeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the font weight of the legend titles.
        /// </summary>
        public FontWeight LegendTitleFontWeight
        {
            get
            {
                return GetValue(LegendTitleFontWeightProperty);
            }

            set
            {
                SetValue(LegendTitleFontWeightProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the legend is visible.
        /// </summary>
        public bool IsLegendVisible
        {
            get
            {
                return GetValue(IsLegendVisibleProperty);
            }

            set
            {
                SetValue(IsLegendVisibleProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="OxyPlot.EdgeRenderingMode"/> for the legend.
        /// </summary>
        public EdgeRenderingMode EdgeRenderingMode
        {
            get
            {
                return GetValue(EdgeRenderingModeProperty);
            }

            set
            {
                SetValue(EdgeRenderingModeProperty, value);
            }
        }
    }
}