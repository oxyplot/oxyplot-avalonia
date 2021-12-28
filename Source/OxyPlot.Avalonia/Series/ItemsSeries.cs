// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItemsSeries.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Abstract base class for series that use X and Y axes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OxyPlot.Avalonia
{
    /// <summary>
    /// Abstract base class for series that use X and Y axes.
    /// </summary>
    public abstract class ItemsSeries : Series
    {
        /// <summary>
        /// Synchronizes the properties.
        /// </summary>
        /// <param name="series">The series.</param>
        protected override void SynchronizeProperties(OxyPlot.Series.Series series)
        {
            base.SynchronizeProperties(series);
            var s = (OxyPlot.Series.ItemsSeries)series;
            // TODO: BarSeriesManager gets very unhappy when this is not initialised (i.e. empty collection): easier to fix in Core?
            s.ItemsSource = Items;
        }
    }
}