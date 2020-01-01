// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TwoColorAreaSeries.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   The Avalonia wrapper for OxyPlot.TwoColorAreaSeries.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using Avalonia;

namespace OxyPlot.Avalonia
{
    using global::Avalonia.Media;

    /// <summary>
    /// The Avalonia wrapper for OxyPlot.TwoColorAreaSeries.
    /// </summary>
    public class TwoColorAreaSeries : AreaSeries
    {
        /// <summary>
        /// Identifies the <see cref="Dashes2"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<double[]> Dashes2Property = AvaloniaProperty.Register<TwoColorAreaSeries, double[]>(nameof(Dashes2));
        
        /// <summary>
        /// Identifies the <see cref="Fill"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<Color> FillProperty = AvaloniaProperty.Register<TwoColorAreaSeries, Color>(nameof(Fill), MoreColors.Automatic);

        /// <summary>
        /// Identifies the <see cref="Fill2"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<Color> Fill2Property = AvaloniaProperty.Register<TwoColorAreaSeries, Color>(nameof(Fill2), MoreColors.Automatic);

        /// <summary>
        /// Identifies the <see cref="LineStyle2"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<LineStyle> LineStyle2Property = AvaloniaProperty.Register<TwoColorAreaSeries, LineStyle>(nameof(LineStyle2));

        /// <summary>
        /// Identifies the <see cref="Limit"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty LimitProperty = AvaloniaProperty.Register<TwoColorAreaSeries, double>(nameof(Limit));
        
        /// <summary>
        /// Identifies the <see cref="MarkerFill2"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<Color> MarkerFill2Property = AvaloniaProperty.Register<TwoColorAreaSeries, Color>(nameof(MarkerFill2), MoreColors.Automatic);

        /// <summary>
        /// Identifies the <see cref="MarkerStroke2"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<Color> MarkerStroke2Property = AvaloniaProperty.Register<TwoColorAreaSeries, Color>(nameof(MarkerStroke2), MoreColors.Automatic);

        /// <summary>
        /// Initializes a new instance of the <see cref = "TwoColorAreaSeries" /> class.
        /// </summary>
        public TwoColorAreaSeries()
        {
            InternalSeries = new OxyPlot.Series.TwoColorAreaSeries();
        }

        /// <summary>
        /// Gets or sets the dash array for the rendered line that is below the limit (overrides <see cref="LineStyle" />).
        /// </summary>
        public double[] Dashes2
        {
            get
            {
                return GetValue(Dashes2Property);
            }
            set
            {
                SetValue(Dashes2Property, value);
            }
        }

        /// <summary>
        /// Gets or sets Fill above the limit line.
        /// </summary>
        public Color Fill
        {
            get
            {
                return GetValue(FillProperty);
            }

            set
            {
                SetValue(FillProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets Fill below the limit line.
        /// </summary>
        public Color Fill2
        {
            get
            {
                return GetValue(Fill2Property);
            }

            set
            {
                SetValue(Fill2Property, value);
            }
        }

        /// <summary>
        /// Gets or sets Marker Fill which is below the limit line.
        /// </summary>
        public Color MarkerFill2
        {
            get
            {
                return GetValue(MarkerFill2Property);
            }

            set
            {
                SetValue(MarkerFill2Property, value);
            }
        }

        /// <summary>
        /// Gets or sets Marker Stroke which is below the limit line.
        /// </summary>
        public Color MarkerStroke2
        {
            get
            {
                return GetValue(MarkerStroke2Property);
            }

            set
            {
                SetValue(MarkerStroke2Property, value);
            }
        }
        
        /// <summary>
        /// Gets or sets the line style for the part of the line that is below the limit.
        /// </summary>
        public LineStyle LineStyle2
        {
            get
            {
                return (LineStyle)this.GetValue(LineStyle2Property);
            }

            set
            {
                this.SetValue(LineStyle2Property, value);
            }
        }

        /// <summary>
        /// Gets or sets a baseline for the series.
        /// </summary>
        public double Limit
        {
            get
            {
                return (double)this.GetValue(LimitProperty);
            }

            set
            {
                this.SetValue(LimitProperty, value);
            }
        }

        /// <summary>
        /// Synchronizes the properties.
        /// </summary>
        /// <param name="series">The series.</param>
        protected override void SynchronizeProperties(OxyPlot.Series.Series series)
        {
            base.SynchronizeProperties(series);
            var s = (OxyPlot.Series.TwoColorAreaSeries)series;            
            s.Fill = Fill.ToOxyColor();
            s.Fill2 = Fill2.ToOxyColor();
            s.MarkerFill2 = MarkerFill2.ToOxyColor();
            s.MarkerStroke2 = MarkerStroke2.ToOxyColor();
            s.Limit = Limit;
            s.Dashes2 = Dashes2?.ToArray();
            s.LineStyle2 = LineStyle2;
        }

        static TwoColorAreaSeries()
        {
            FillProperty.Changed.AddClassHandler<TwoColorAreaSeries>(AppearanceChanged);
            Fill2Property.Changed.AddClassHandler<TwoColorAreaSeries>(AppearanceChanged);
            MarkerFill2Property.Changed.AddClassHandler<TwoColorAreaSeries>(AppearanceChanged);
            MarkerStroke2Property.Changed.AddClassHandler<TwoColorAreaSeries>(AppearanceChanged);
            LimitProperty.Changed.AddClassHandler<TwoColorAreaSeries>(AppearanceChanged);
            Dashes2Property.Changed.AddClassHandler<TwoColorAreaSeries>(AppearanceChanged);
            LineStyle2Property.Changed.AddClassHandler<TwoColorAreaSeries>(AppearanceChanged);
        }
    }
}