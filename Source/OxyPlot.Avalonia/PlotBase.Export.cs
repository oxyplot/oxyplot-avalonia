// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlotBase.Export.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Represents a control that displays a <see cref="PlotModel" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OxyPlot.Avalonia
{
    using global::Avalonia.Media.Imaging;
    using System;
    using System.IO;

    /// <summary>
    /// Represents a control that displays a <see cref="PlotModel" />.
    /// </summary>
    public partial class PlotBase
    {
        /// <summary>
        /// Saves the PlotView as a bitmap.
        /// </summary>
        /// <param name="stream">Stream to which to write the bitmap.</param>
        public void SaveBitmap(Stream stream)
        {
            SaveBitmap(stream, -1, -1, GetModelForExport().Background);
        }

        /// <summary>
        /// Saves the PlotView as a bitmap.
        /// </summary>
        /// <param name="stream">Stream to which to write the bitmap.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="background">The background.</param>
        public void SaveBitmap(Stream stream, int width, int height, OxyColor background)
        {
            var model = GetModelForExport();

            if (width <= 0)
            {
                width = (int)Bounds.Width;
            }

            if (height <= 0)
            {
                height = (int)Bounds.Height;
            }

            CheckExportSize(width, height);

            if (!background.IsVisible())
            {
                background = Background.ToOxyColor();
            }

            try
            {
                lock (model.SyncRoot)
                {
                    PngExporter.Export(model, stream, width, height, background);
                }
            }
            finally
            {
                // The export recalculated the live model for its own size, so it is restored to the size of the control
                InvalidatePlot(false);
            }
        }

        /// <summary>
        /// Renders the PlotView to a bitmap.
        /// </summary>
        /// <returns>A bitmap.</returns>
        public Bitmap ToBitmap()
        {
            var model = GetModelForExport();
            var width = (int)Bounds.Width;
            var height = (int)Bounds.Height;
            CheckExportSize(width, height);

            var background = model.Background.IsVisible() ? model.Background : Background.ToOxyColor();
            try
            {
                lock (model.SyncRoot)
                {
                    return PngExporter.ExportToBitmap(model, width, height, background);
                }
            }
            finally
            {
                // The export recalculated the live model for its own size, so it is restored to the size of the control
                InvalidatePlot(false);
            }
        }

        /// <summary>
        /// Gets the model for export.
        /// </summary>
        /// <returns>The current model.</returns>
        /// <exception cref="InvalidOperationException">There is no model.</exception>
        private PlotModel GetModelForExport()
        {
            return ActualModel ?? throw new InvalidOperationException("Cannot export a plot without a model: the Model property is not set.");
        }

        /// <summary>
        /// Validates the bitmap size for export.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <exception cref="InvalidOperationException">The size is less than one pixel.</exception>
        private static void CheckExportSize(int width, int height)
        {
            if (width < 1 || height < 1)
            {
                throw new InvalidOperationException(
                    "The control has not been laid out yet or is hidden, so its size is zero. Specify the width and height explicitly in SaveBitmap.");
            }
        }
    }
}
