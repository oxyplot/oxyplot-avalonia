// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserControl1.xaml.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Interaction logic for UserControl1.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace AvaloniaExamples.Examples.UserControlDemo
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : Avalonia.Controls.UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }
    }
}