// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Represents the view-model for the main window.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExampleBrowser
{
    using Avalonia.Data.Converters;

    public class NotNullBooleanConverter : FuncValueConverter<object, bool>
    {
        public NotNullBooleanConverter() : base(obj => obj is not null)
        {
        }
    }
}