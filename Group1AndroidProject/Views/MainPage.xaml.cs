using Group1AndroidProject.Models;
namespace Group1AndroidProject.Views;

public partial class MainPage : ContentPage
{

    ConnectionHelper connectionHelper;

    public MainPage()
	{
		InitializeComponent();
        connectionHelper = new ConnectionHelper();
        GetCurrentLocation();
        GetTheData();
    }


    private void welcomeLabel_Loaded(object sender, EventArgs e)
    {
        welcomeLabel.Text = $"Welcome {OperationParameters.currentUser}";
    }

    public async void GetCurrentLocation()
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
        connectionHelper = new();
        await connectionHelper.SendMyCurrentLocationAsync();
    }
    public void GetTheData()
    {
        connectionHelper = new();
        connectionHelper.GatherSourceData();
    }

}