using ProjektGrupowy.Services;
using System.Net.NetworkInformation;
namespace ProjektGrupowy.Views;

public partial class EntryPage : ContentPage
{

    //flags

    public bool internerConnectionAvailable { get; set; } = true;
    public bool GPSSignalAvailable { get; set; } = true;
    public bool SQLConnectionAvailable { get; set; } = true;
    ConnectionHelper connectionHelper;

    public EntryPage()
	{
		InitializeComponent();
        connectionHelper = new ConnectionHelper();
        CheckConditions();
    }
    #region Events
    private async void enterButton_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(nickEntry.Text))
        {
            await DisplayAlertAsync("Warning", "Please enter a valid nick.", "Ok");
            return;
        }
        OperationParameters.currentUser = nickEntry.Text.Trim();

        connectionHelper = new();
        // Ensure nick exists in table (insert if not exists)
        await connectionHelper.SendEnterQueryAsync();

        // Check if this is a new user (no name field set in DB)
        await connectionHelper.CheckIfUserIsNewAsync();

        if (OperationParameters.newUser)
        {
            // Navigate to registration / edit page where password will be set
            await Shell.Current.GoToAsync(nameof(EditContactPage));
            return;
        }

        // Existing user: ensure password field is visible. If password not yet entered, show it and let user enter password.
        if (!passwordEntry.IsVisible || string.IsNullOrWhiteSpace(passwordEntry.Text))
        {
            passwordEntry.IsVisible = true;
            passwordEntry.Focus();
            return;
        }

        // Compare provided password with stored hash
        var storedHash = await connectionHelper.GetPassHashAsync(OperationParameters.currentUser);
        if (string.IsNullOrEmpty(storedHash))
        {
            // No password set for this user; route to edit page to create one
            await Shell.Current.GoToAsync(nameof(EditContactPage));
            return;
        }

        var providedHash = ConnectionHelper.ComputeHash(passwordEntry.Text ?? string.Empty);
        if (providedHash == storedHash)
        {
            await Shell.Current.GoToAsync(nameof(MainPage));
        }
        else
        {
            await DisplayAlertAsync("Login failed", "Incorrect password.", "OK");
        }
    }
    #endregion


    #region Pre-Check Logic
    private async void CheckConditions()
    {
        await IsInternetAvailableAsync();
        await IsGpsAvailableAsync();
        await IsSQLAvailableAsync();

        if (internerConnectionAvailable == false || GPSSignalAvailable == false || SQLConnectionAvailable == false)
        {
            await DisplayAlertAsync("Error", $"The application requires GPS,Internet,and SQL base connection to be operational for working /n { OperationParameters.errorDisplayer}", "Quit");
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
        //ConnectionHelper connectionHelper = new();
        SQLConnectionAvailable = connectionHelper.CheckConnection();

        return Task.CompletedTask;
    }

    #endregion
}