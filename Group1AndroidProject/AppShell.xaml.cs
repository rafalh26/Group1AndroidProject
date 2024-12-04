using Group1AndroidProject.Views;

namespace Group1AndroidProject
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("Attractions", typeof(Attractions));
        }
    }
}
