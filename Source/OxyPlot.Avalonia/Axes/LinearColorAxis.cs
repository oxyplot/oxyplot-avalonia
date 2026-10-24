// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LinearColorAxis.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Provides a Avalonia wrapper for the <see cref="OxyPlot.Axes.LinearColorAxis" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Avalonia;

namespace OxyPlot.Avalonia
{
    using global::Avalonia.Media;
    using global::Avalonia.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Provides a Avalonia wrapper for the <see cref="Axes.LinearColorAxis" />.
    /// </summary>
    public class LinearColorAxis : Axis
    {
        /// <summary>
        /// Identifies the <see cref="GradientStopsProperty"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<IList<GradientStop>> GradientStopsProperty = AvaloniaProperty.Register<LinearColorAxis, IList<GradientStop>>(nameof(GradientStops));

        /// <summary>
        /// Identifies the <see cref="HighColorProperty"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<Color> HighColorProperty = AvaloniaProperty.Register<LinearColorAxis, Color>(nameof(HighColor), Colors.White);

        /// <summary>
        /// Identifies the <see cref="LowColorProperty"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<Color> LowColorProperty = AvaloniaProperty.Register<LinearColorAxis, Color>(nameof(LowColor), Colors.Black);

        /// <summary>
        /// Identifies the <see cref="PaletteSizeProperty"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<int> PaletteSizeProperty = AvaloniaProperty.Register<LinearColorAxis, int>(nameof(PaletteSize), 20, validate: ValidatePaletteSize);

        /// <summary>
        /// The default palette of the core. It is returned when there are no gradient stops.
        /// </summary>
        private readonly OxyPalette defaultPalette;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearColorAxis"/> class.
        /// </summary>
        public LinearColorAxis()
        {
            InternalAxis = new Axes.LinearColorAxis();
            defaultPalette = ((Axes.LinearColorAxis)InternalAxis).Palette;

            // The value is set as the current one so that it does not override style setters.
            SetCurrentValue(GradientStopsProperty, new List<GradientStop>());
        }

        /// <summary>
        /// Gets or sets the palette size.
        /// </summary>
        public int PaletteSize
        {
            get
            {
                return GetValue(PaletteSizeProperty);
            }

            set
            {
                SetValue(PaletteSizeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the high color.
        /// </summary>
        public Color HighColor
        {
            get
            {
                return GetValue(HighColorProperty);
            }

            set
            {
                SetValue(HighColorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the low color.
        /// </summary>
        public Color LowColor
        {
            get
            {
                return GetValue(LowColorProperty);
            }

            set
            {
                SetValue(LowColorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the gradient stops.
        /// </summary>
        [Content]
        public IList<GradientStop> GradientStops
        {
            get
            {
                return GetValue(GradientStopsProperty);
            }

            set
            {
                SetValue(GradientStopsProperty, value);
            }
        }

        /// <summary>
        /// Creates the model.
        /// </summary>
        /// <returns>
        /// An axis object.
        /// </returns>
        public override Axes.Axis CreateModel()
        {
            SynchronizeProperties();
            return InternalAxis;
        }

        /// <summary>
        /// Synchronizes the properties.
        /// </summary>
        protected override void SynchronizeProperties()
        {
            base.SynchronizeProperties();
            if (InternalAxis is not Axes.LinearColorAxis axis)
            {
                throw new InvalidOperationException("The internal axis must be of type LinearColorAxis.");
            }

            if (GradientStops != null && GradientStops.Count >= 2)
            {
                axis.Palette = Interpolate(GradientStops.ToList(), PaletteSize);
            }
            else if (GradientStops != null && GradientStops.Count == 1)
            {
                // A single stop: a palette of one color, but of the requested size.
                axis.Palette = new OxyPalette(
                    Enumerable.Repeat(GradientStops[0].Color.ToOxyColor(), Math.Max(1, PaletteSize)).ToList());
            }
            else
            {
                // No stops: return the palette of the core, otherwise the map would become monochrome.
                axis.Palette = defaultPalette;
            }

            axis.HighColor = HighColor.ToOxyColor();
            axis.LowColor = LowColor.ToOxyColor();
            axis.Minimum = Minimum;
            axis.Maximum = Maximum;
        }

        /// <summary>
        /// Translates a collection of <see cref="GradientStop" /> to an <see cref="OxyPalette" />.
        /// </summary>
        /// <param name="stops">
        /// The gradient stops collection to convert.
        /// </param>
        /// <param name="paletteSize">
        /// The palette size.
        /// </param>
        /// <returns>
        /// The interpolated <see cref="OxyPalette"/>.
        /// </returns>
        private static OxyPalette Interpolate(List<GradientStop> stops, int paletteSize)
        {
            Debug.Assert(stops.Count >= 2, "Can't interpolate less than 2 gradient stops.");
            Debug.Assert(paletteSize > 0, "Palette size must be non-zero positive number.");

            stops.Sort((x1, x2) => x1.Offset.CompareTo(x2.Offset));

            var first = stops[0].Offset;
            var last = stops[stops.Count - 1].Offset;
            var range = last - first;

            // All stops are at one point or a single color is needed: there is nothing to interpolate,
            // but the requested palette size must still be honored.
            if (range <= 0 || paletteSize < 2)
            {
                return new OxyPalette(
                    Enumerable.Repeat(stops[0].Color.ToOxyColor(), Math.Max(1, paletteSize)).ToList());
            }

            var palette = new List<OxyColor>(paletteSize);
            var segment = 0;

            for (int i = 0; i < paletteSize; i++)
            {
                var position = first + (range * i / (paletteSize - 1));

                while (segment < stops.Count - 2 && position > stops[segment + 1].Offset)
                {
                    segment++;
                }

                var start = stops[segment];
                var end = stops[segment + 1];
                var span = end.Offset - start.Offset;
                // Stops with the same offset: take the start color, no division by zero.
                var t = span > 0 ? (position - start.Offset) / span : 0.0;

                palette.Add(OxyColor.Interpolate(start.Color.ToOxyColor(), end.Color.ToOxyColor(), t));
            }

            return new OxyPalette(palette);
        }

        /// <summary>
        /// Validates the palette size.
        /// </summary>
        /// <param name="value">
        /// The property value.
        /// </param>
        /// <returns>
        /// The validation result.
        /// </returns>
        private static bool ValidatePaletteSize(int value)
        {
            return value >= 1;
        }

        static LinearColorAxis()
        {
            GradientStopsProperty.Changed.AddClassHandler<LinearColorAxis>(AppearanceChanged);
            HighColorProperty.Changed.AddClassHandler<LinearColorAxis>(AppearanceChanged);
            LowColorProperty.Changed.AddClassHandler<LinearColorAxis>(AppearanceChanged);
            PaletteSizeProperty.Changed.AddClassHandler<LinearColorAxis>(AppearanceChanged);
        }
    }
}