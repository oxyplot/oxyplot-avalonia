// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Category.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Represents a category of examples.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExampleBrowser
{
    using ExampleLibrary;
    using System;
    using System.Collections.Generic;

    public class Category
    {
        public Category(string key, List<ExampleInfo> examples)
        {
            this.Key = key;
            this.Examples = examples ?? throw new ArgumentNullException(nameof(examples));
        }

        public string Key { get; }
        public List<ExampleInfo> Examples { get; }
    }
}