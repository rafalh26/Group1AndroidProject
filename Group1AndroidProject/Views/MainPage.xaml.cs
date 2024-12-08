namespace Group1AndroidProject.Views;
using System.Net.NetworkInformation;
using Group1AndroidProject.ConnectSQL;
using Group1AndroidProject.Parameters;
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
    private async void enterButton_Clicked(object sender, EventArgs e)
    {
        // Ensure nickEntry.Text is not null or empty before proceeding
        if (string.IsNullOrWhiteSpace(nickEntry.Text))
        {
            await DisplayAlert("Warning", "Please enter a valid nick.", "Ok");
            return;
        }

        // Using the ConnectionHelper in a `using` block
        using (ConnectionHelper connectionHelper = new ConnectionHelper())
        {
            //uncomment for debug
            //await DisplayAlert("SQL MESSAGE:", connectionHelper.sendEnterQuery(nickEntry.Text, this), "ok");
            connectionHelper.sendEnterQuery(nickEntry.Text, this);
        }
        if (nickEntry.Text != string.Empty)
        {
            OperationParameters.currentUser = nickEntry.Text;
            await Shell.Current.GoToAsync(nameof(ContactsListInRange));
        }
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