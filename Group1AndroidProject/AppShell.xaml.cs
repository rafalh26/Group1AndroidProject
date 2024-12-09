using Group1AndroidProject.Views;
namespace Group1AndroidProject
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(EditContactPage), typeof(EditContactPage));
            Routing.RegisterRoute(nameof(ContactsListInRange), typeof(ContactsListInRange));
            Routing.RegisterRoute(nameof(ContactDetailsPage), typeof(ContactDetailsPage));
        }
    }
}
