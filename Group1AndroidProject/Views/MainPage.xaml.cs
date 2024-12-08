namespace Group1AndroidProject.Views;
using System.Net.NetworkInformation;
using Group1AndroidProject.ConnectSQL;
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
    private void nameEntry_Loaded(object sender, EventArgs e)
    {
        nameEntry.Text = "";
        initializedEntry = true;
    }
    #endregion
    #region LogicFunctions
    bool doubleCheck = false;
    private async void CheckConditions()
    {
        await IsInternetAvailableAsync();
        await IsGpsAvailableAsync();
        await IsSQLAvailableAsync();

        if (internerConnectionAvailable == false || GPSSignalAvailable == false || SQLConnectionAvailable == false)
        {
            DisplayAlert("Error", "The application requires GPS,Interner,and SQL base connection to be operational for working", "Quit");
            await Task.Delay(5000);
            Application.Current.Quit();
        }
    }
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
    private CancellationTokenSource _cancelTokenSource;
    private bool _isCheckingLocation;

    public async Task IsGpsAvailableAsync()
    {
        try
        {
            _isCheckingLocation = true;

            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

            _cancelTokenSource = new CancellationTokenSource();

            Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

            if (location != null)
                GPSSignalAvailable = true;
        }
        // Catch one of the following exceptions:
        //   FeatureNotSupportedException
        //   FeatureNotEnabledException
        //   PermissionException
        catch (Exception ex)
        {
            GPSSignalAvailable = false;
        }
        finally
        {
            _isCheckingLocation = false;
        }
    }
    public void CancelRequest()
    {
        if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
            _cancelTokenSource.Cancel();
    }


    //SQL Connection check

    private async Task IsSQLAvailableAsync()
    {
        ConnectionHelper connectionHelper = new();
        SQLConnectionAvailable = connectionHelper.CheckConnection();
    }

    #endregion
}