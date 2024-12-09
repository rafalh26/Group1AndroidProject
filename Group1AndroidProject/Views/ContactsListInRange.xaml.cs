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
        _ = GetCurrentLocation();
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
        welcomeLabel.Text = $"Welcome {OperationParameters.currentUser}";
        CheckIfNewUser();
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