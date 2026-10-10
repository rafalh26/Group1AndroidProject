using ProjektGrupowy.Views;
namespace ProjektGrupowy
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            //Routing.RegisterRoute(nameof(EntryPage), typeof(EntryPage));
            Routing.RegisterRoute(nameof(UserInfoPage), typeof(UserInfoPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        }
    }
}
