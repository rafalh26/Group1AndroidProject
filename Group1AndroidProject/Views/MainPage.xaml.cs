using Group1AndroidProject.Models;
namespace Group1AndroidProject.Views;

public partial class MainPage : ContentPage
{

    ConnectionHelper connectionHelper;

    public MainPage()
	{
		InitializeComponent();
        connectionHelper = new ConnectionHelper();
        InitializeClock();
    }

    private async void InitializeClock()
    {
        //OperationParameters.currentUser = await connectionHelper.SendEnterQueryAsync(OperationParameters.currentUser, this);
        //CheckIfNewUser();
        GetCurrentLocation();
    }

    private void welcomeLabel_Loaded(object sender, EventArgs e)
    {
        welcomeLabel.Text = $"Welcome {OperationParameters.currentUser}";
    }

    public async Task GetCurrentLocation()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(2));
            OperationParameters.MyCurrentLocation = await Geolocation.Default.GetLocationAsync(request);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error during location reading:", ex.Message, "OK");
        }
        await connectionHelper.SendMyCurrentLocationAsync();
    }
}