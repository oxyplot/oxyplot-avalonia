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
    using System.IO;

    /// <summary>
    /// Represents a control that displays a <see cref="PlotModel" />.
    /// </summary>
    public partial class PlotView
    {
        /// <summary>
        /// Saves the PlotView as a bitmap.
        /// </summary>
        /// <param name="stream">Stream to which to write the bitmap.</param>
        public void SaveBitmap(Stream stream)
        {
            this.SaveBitmap(stream, -1, -1, this.ActualModel.Background);
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
            if (width <= 0)
            {
                width = (int)this.Bounds.Width;
            }

            if (height <= 0)
            {
                height = (int)this.Bounds.Height;
            }

            if (!background.IsVisible())
            {
                background = this.Background.ToOxyColor();
            }

            PngExporter.Export(this.ActualModel, stream, width, height, background);
        }

        /// <summary>
        /// Renders the PlotView to a bitmap.
        /// </summary>
        /// <returns>A bitmap.</returns>
        public Bitmap ToBitmap()
        {
            var background = this.ActualModel.Background.IsVisible() ? this.ActualModel.Background : this.Background.ToOxyColor();
            return PngExporter.ExportToBitmap(this.ActualModel, (int)this.Bounds.Width, (int)this.Bounds.Height, background);
        }
    }
}
