namespace Group1AndroidProject.Views;
using Group1AndroidProject.ConnectSQL;
using Group1AndroidProject.Parameters;
using System.Net.NetworkInformation;
public partial class MainPage : ContentPage
{
    //flags
    public bool initializedEntry { get; set; } = false;
    public bool internerConnectionAvailable { get; set; } = true;
    public bool GPSSignalAvailable { get; set; } = true;
    public bool SQLConnectionAvailable { get; set; } = true;

    //Initialization
    public MainPage()
    {
        InitializeComponent();
        CheckConditions();
    }

    #region EventHandlers
    private void nickEntry_Loaded(object sender, EventArgs e)
    {
        nickEntry.Text = "";
        initializedEntry = true;
    }


    #endregion
    #region LogicFunctions
    private async void CheckConditions()
    {
        await IsInternetAvailableAsync();
        await IsGpsAvailableAsync();
        await IsSQLAvailableAsync();

        if (internerConnectionAvailable == false || GPSSignalAvailable == false || SQLConnectionAvailable == false)
        {
            await DisplayAlert("Error", "The application requires GPS,Interner,and SQL base connection to be operational for working", "Quit");
            await Task.Delay(5000);
            Application.Current.Quit();
        }
    }
    //Is internet avaiable Check
    private async Task<bool> IsInternetAvailableAsync()
    {
        try
        {
            Ping myPing = new Ping();
            PingReply reply = await myPing.SendPingAsync("google.com");
            internerConnectionAvailable = true;
            return reply.Status == IPStatus.Success;
        }
        catch (Exception)
        {
            internerConnectionAvailable = false;
            return false;
        }
    }
    //GPS checking according to MAUI docs
    public async Task IsGpsAvailableAsync()
    {
        try
        {
            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

            Location? location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
                GPSSignalAvailable = true;
        }
        // Catch one of the following exceptions:
        //   FeatureNotSupportedException
        //   FeatureNotEnabledException
        //   PermissionException
        catch (Exception)
        {
            GPSSignalAvailable = false;
        }
    }
    //SQL Connection check
    private Task IsSQLAvailableAsync()
    {
        ConnectionHelper connectionHelper = new();
        SQLConnectionAvailable = connectionHelper.CheckConnection();
        return Task.CompletedTask;
    }

    #endregion

    private async void enterButton_Clicked(object sender, EventArgs e)
    {
        {
            // Ensure nickEntry.Text is not null or empty before proceeding
            if (string.IsNullOrWhiteSpace(nickEntry.Text))
            {
                await DisplayAlert("Warning", "Please enter a valid nick.", "Ok");
                return;
            }
            OperationParameters.currentUser = nickEntry.Text;
            // Using the ConnectionHelper in a `using` block guarantee object disposal and therefore connection termination
            await Shell.Current.GoToAsync(nameof(ContactsListInRange));
        }
    }
}