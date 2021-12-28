// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Example.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using Avalonia;
using Avalonia.Platform;

namespace AvaloniaExamples
{
    using Avalonia.Media.Imaging;
    using System;
    using System.Diagnostics;

    public class Example
    {
        public Example(Type mainWindowType, string title = null, string description = null)
        {
            this.MainWindowType = mainWindowType;
            this.Title = title ?? mainWindowType.Namespace.Split('.').Last();
            this.Description = description;
            try
            {
                var uri = new Uri("resm:AvaloniaExamples.Images." + this.ThumbnailFileName);
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                var stream = assets.Open(uri);
                this.Thumbnail = new Bitmap(stream);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public string Title { get; }
        public string Description { get; set; }
        private Type MainWindowType { get; }

        public IBitmap Thumbnail { get; set; }

        public string ThumbnailFileName
        {
            get
            {
                return this.Title + ".png";
            }
        }

        public override string ToString()
        {
            return this.Title;
        }

        public Avalonia.Controls.Window Create()
        {
            return Activator.CreateInstance(this.MainWindowType) as Avalonia.Controls.Window;
        }
    }
}