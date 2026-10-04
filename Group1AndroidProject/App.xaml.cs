namespace Group1AndroidProject
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // AppShell is created in CreateWindow for multi-window support on modern MAUI
            // Keep constructor minimal per MAUI recommendations
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
