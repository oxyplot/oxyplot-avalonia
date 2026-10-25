// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogarithmicAxis.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   This is a Avalonia wrapper of OxyPlot.LogarithmicAxis.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Avalonia;

namespace OxyPlot.Avalonia
{
    /// <summary>
    /// This is a Avalonia wrapper of OxyPlot.LogarithmicAxis.
    /// </summary>
    public class LogarithmicAxis : Axis
    {
        /// <summary>
        /// Identifies the <see cref="Base"/> dependency property.
        /// </summary>
        /// <value>The logarithmic base.</value>
        public static readonly StyledProperty<double> BaseProperty = AvaloniaProperty.Register<LogarithmicAxis, double>(nameof(Base), 10.0, validate: ValidateBase);

        /// <summary>
        /// Identifies the <see cref="PowerPadding"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<bool> PowerPaddingProperty = AvaloniaProperty.Register<LogarithmicAxis, bool>(nameof(PowerPadding), true);

        /// <summary>
        /// Initializes a new instance of the <see cref = "LogarithmicAxis" /> class.
        /// </summary>
        public LogarithmicAxis()
        {
            InternalAxis = new Axes.LogarithmicAxis();
        }

        /// <summary>
        /// Gets or sets Base.
        /// </summary>
        public double Base
        {
            get
            {
                return GetValue(BaseProperty);
            }

            set
            {
                SetValue(BaseProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ActualMaximum and ActualMinimum values should be padded to the nearest power of the Base.
        /// </summary>
        public bool PowerPadding
        {
            get
            {
                return GetValue(PowerPaddingProperty);
            }

            set
            {
                SetValue(PowerPaddingProperty, value);
            }
        }

        /// <summary>
        /// Creates the internal axis.
        /// </summary>
        /// <returns>The internal axis.</returns>
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
            var a = (Axes.LogarithmicAxis)InternalAxis;
            a.Base = Base;
            a.PowerPadding = PowerPadding;
        }

        /// <summary>
        /// Validates the logarithm base. With a base that is not greater than one or is not a number
        /// the core cannot compute the axis ticks and rendering hangs.
        /// </summary>
        /// <param name="value">The base.</param>
        /// <returns><c>true</c> if the value is valid.</returns>
        private static bool ValidateBase(double value)
        {
            return double.IsFinite(value) && value > 1;
        }

        static LogarithmicAxis()
        {
            // A default value instead of an assignment in the constructor: a local value would override styles
            FilterMinValueProperty.OverrideDefaultValue<LogarithmicAxis>(0.0);

            BaseProperty.Changed.AddClassHandler<LogarithmicAxis>(DataChanged);
            PowerPaddingProperty.Changed.AddClassHandler<LogarithmicAxis>(DataChanged);
        }
    }
}