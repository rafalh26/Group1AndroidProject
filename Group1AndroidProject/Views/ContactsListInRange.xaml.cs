using Group1AndroidProject.Parameters;
using Group1AndroidProject.ConnectSQL;
namespace Group1AndroidProject.Views;

public partial class ContactsListInRange : ContentPage
{
    ConnectionHelper connectionHelper;
    bool isCurrentUserNew=false;

    public ContactsListInRange()
    {
        InitializeComponent();
        connectionHelper = new ConnectionHelper();
    }
    private void CheckIfNewUser()
    {
        isCurrentUserNew = connectionHelper.IsTheUserNew();
        if (isCurrentUserNew)
        {
           Shell.Current.GoToAsync(nameof(EditContactPage));
        }

    }

    private void welcomeLabel_Loaded(object sender, EventArgs e)
    {
        welcomeLabel.Text = $"Welcome dear {OperationParameters.currentUser}";
    }

    private async void ContentPage_Appearing(object sender, EventArgs e)
    {
        await GetCurrentLocation();
        CheckIfNewUser();
    }
    public async Task GetCurrentLocation()
    {
        GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best,TimeSpan.FromSeconds(10));



    }
}