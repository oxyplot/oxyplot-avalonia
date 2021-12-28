using System;
using System.Collections.Generic;
using System.Text;

namespace OxyPlot.Avalonia
{
    /// <summary>
    /// Represents a control that displays a <see cref="PlotModel" />.
    /// </summary>
    public interface IPlot
    {
        /// <summary>
        /// Signals that the appearance of an element has changed.
        /// </summary>
        /// <param name="element">The element whose appearance has changed.</param>
        void ElementAppearanceChanged(object element);

        /// <summary>
        /// Signals that the data of an element has changed.
        /// </summary>
        /// <param name="element">The element whose data has changed.</param>
        void ElementDataChanged(object element);
    }
}
