using Group1AndroidProject.ConnectSQL;
using Group1AndroidProject.Parameters;
namespace Group1AndroidProject.Views;

public partial class ContactsListInRange : ContentPage
{
    ConnectionHelper connectionHelper;
    bool isCurrentUserNew = false;

    public ContactsListInRange()
    {
        InitializeComponent();
        connectionHelper = new ConnectionHelper();
        InitializeClock();
    }
    private async void InitializeClock()
    {
        await Task.Delay(2000);
        OperationParameters.currentUser = await connectionHelper.SendEnterQueryAsync(OperationParameters.currentUser, this);
        CheckIfNewUser();
        GetCurrentLocation();
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
        welcomeLabel.Text = $"Welcome {OperationParameters.currentUser}";
    }

    public async Task GetCurrentLocation()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(1));
            OperationParameters.MyCurrentLocation = await Geolocation.Default.GetLocationAsync(request);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error during location reading:", ex.Message, "OK");
        }
        await connectionHelper.SendMyCurrentLocationAsync();
    }
}