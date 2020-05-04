// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AvaloniaExamples.Examples.RealtimeDemo
{
    using Avalonia.Threading;
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    public enum SimulationType
    {
        Waves,
        TimeSimulation
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public PlotModel PlotModel
        {
            get
            {
                return plotModel;
            }
            set
            {
                plotModel = value;
                RaisePropertyChanged("PlotModel");
            }
        }

        private PlotModel plotModel;

        public int TotalNumberOfPoints
        {
            get
            {
                return totalNumberOfPoints;
            }
            set
            {
                totalNumberOfPoints = value;
                RaisePropertyChanged("TotalNumberOfPoints");
            }
        }

        private int totalNumberOfPoints = 0;

        public SimulationType SimulationType
        {
            get
            {
                return simulationType;
            }

            set
            {
                simulationType = value;
                RaisePropertyChanged("SimulationType");
            }
        }

        private SimulationType simulationType;

        private readonly DispatcherTimer timer;
        private readonly Stopwatch watch = new Stopwatch();
        private const int UpdateInterval = 33;
        private Func<double, double, double, double> Function { get; set; }

        public MainViewModel()
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(UpdateInterval) };
            timer.Tick += Timer_Tick;

            Function = (t, x, a) => Math.Cos(t * a) * (x == 0 ? 1 : Math.Sin(x * a) / x);
            SimulationType = SimulationType.Waves;
            SetupModel();

            watch.Start();
            timer.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            Dispatcher.UIThread.InvokeAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property)));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateModel();

            Dispatcher.UIThread.InvokeAsync(() => PlotModel.InvalidatePlot(true), DispatcherPriority.Background);
        }

        private void SetupModel()
        {
            PlotModel = new PlotModel();
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = -2, Maximum = 2 });

            var numberOfSeries = SimulationType == SimulationType.TimeSimulation ? 1 : 20;
            for (int i = 0; i < numberOfSeries; i++)
            {
                PlotModel.Series.Add(new LineSeries { LineStyle = LineStyle.Solid });
            }

            RaisePropertyChanged("PlotModel");
        }

        private void UpdateModel()
        {
            double t = watch.ElapsedMilliseconds * 0.001;
            int n = 0;

            for (int i = 0; i < PlotModel.Series.Count; i++)
            {
                var s = (LineSeries)PlotModel.Series[i];

                switch (SimulationType)
                {
                    case SimulationType.TimeSimulation:
                        {
                            double x = s.Points.Count > 0 ? s.Points[s.Points.Count - 1].X + 1 : 0;
                            if (s.Points.Count >= 200)
                                s.Points.RemoveAt(0);
                            double y = 0;
                            int m = 80;
                            for (int j = 0; j < m; j++)
                                y += Math.Cos(0.001 * x * j * j);
                            y /= m;
                            s.Points.Add(new DataPoint(x, y));
                            break;
                        }

                    case SimulationType.Waves:
                        s.Points.Clear();
                        double a = 0.5 + i * 0.05;
                        for (double x = -5; x <= 5; x += 0.01)
                        {
                            s.Points.Add(new DataPoint(x, Function(t, x, a)));
                        }

                        break;
                }

                n += s.Points.Count;
            }

            if (TotalNumberOfPoints != n)
            {
                TotalNumberOfPoints = n;
            }

            RaisePropertyChanged("PlotModel");
        }
    }
}