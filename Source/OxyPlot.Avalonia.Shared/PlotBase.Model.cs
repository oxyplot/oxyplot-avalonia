using Avalonia;

namespace OxyPlot.Avalonia
{
    public partial class PlotBase
    {
        /// <summary>
        /// Identifies the <see cref="Controller"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<IPlotController> ControllerProperty = AvaloniaProperty.Register<PlotBase, IPlotController>(nameof(Controller));

        /// <summary>
        /// Identifies the <see cref="Model"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<PlotModel> ModelProperty = AvaloniaProperty.Register<PlotBase, PlotModel>(nameof(Model), null);

        /// <summary>
        /// The model lock.
        /// </summary>
        private readonly object modelLock = new();

        /// <summary>
        /// The current model (synchronized with the <see cref="Model" /> property, but can be accessed from all threads.
        /// </summary>
        private PlotModel currentModel;

        /// <summary>
        /// The default plot controller.
        /// </summary>
        private IPlotController defaultController;

        static PlotBase()
        {
            ModelProperty.Changed.AddClassHandler<PlotBase>(ModelChanged);
            PaddingProperty.OverrideMetadata(typeof(PlotBase), new StyledPropertyMetadata<Thickness>(new Thickness(8)));
            PaddingProperty.Changed.AddClassHandler<PlotBase>(AppearanceChanged);
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public PlotModel Model
        {
            get => this.GetValue(ModelProperty);
            set => this.SetValue(ModelProperty, value);
        }

        /// <summary>
        /// Gets or sets the Plot controller.
        /// </summary>
        /// <value>The Plot controller.</value>
        public IPlotController Controller
        {
            get => this.GetValue(ControllerProperty);
            set => this.SetValue(ControllerProperty, value);
        }

        Model IView.ActualModel => this.ActualModel;

        IController IView.ActualController => this.ActualController;

        /// <summary>
        /// Gets the actual model.
        /// </summary>
        /// <value>The actual model.</value>
        public virtual PlotModel ActualModel => this.currentModel;

        /// <summary>
        /// Gets the actual PlotView controller.
        /// </summary>
        /// <value>The actual PlotView controller.</value>
        public virtual IPlotController ActualController => this.Controller ?? (this.defaultController ??= new PlotController());

        /// <summary>
        /// Called when the model is changed.
        /// </summary>
        private void OnModelChanged()
        {
            lock (this.modelLock)
            {
                if (this.currentModel != null)
                {
                    ((IPlotModel)this.currentModel).AttachPlotView(null);
                    this.currentModel = null;
                }

                if (this.Model != null)
                {
                    ((IPlotModel)this.Model).AttachPlotView(null); // detach so we can re-attach
                    ((IPlotModel)this.Model).AttachPlotView(this);
                    this.currentModel = this.Model;
                }
            }

            this.InvalidatePlot();
        }

        /// <summary>
        /// Called when the model is changed.
        /// </summary>
        /// <param name="d">The sender.</param>
        /// <param name="e">The <see cref="AvaloniaPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void ModelChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((PlotBase)d).OnModelChanged();
        }

        /// <summary>
        /// Called when the visual appearance is changed.
        /// </summary>
        protected void OnAppearanceChanged()
        {
            this.InvalidatePlot(false);
        }

        /// <summary>
        /// Called when the visual appearance is changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="AvaloniaPropertyChangedEventArgs" /> instance containing the event data.</param>
        protected static void AppearanceChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((PlotBase)d).OnAppearanceChanged();
        }
    }
}
