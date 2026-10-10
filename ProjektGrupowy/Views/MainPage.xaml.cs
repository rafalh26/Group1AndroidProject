using ProjektGrupowy.Services;

namespace ProjektGrupowy.Views;

public partial class MainPage : ContentPage
{

    ConnectionHelper? connectionHelper;
    LoggingService _loggingService = new();
    private bool _isLogging = false;
    private int? _currentTrackId = null;
    //public ObservableCollection<Models.User> ContactsInRange { get; set; }

public MainPage()
	{
		InitializeComponent();
		connectionHelper = new ConnectionHelper();
	}    

	// Note: StartUpCommandsAsync is invoked from OnAppearing to ensure page is loaded before running startup tasks.
	// If you need startup work earlier, call StartUpCommandsAsync() from OnAppearing or use a loaded event.

    public async Task StartUpCommandsAsync()
    {
        await GetCurrentLocation();
    }


    private void welcomeLabel_Loaded(object sender, EventArgs e)
    {
        // keep for compatibility; primary set happens in OnAppearing
        welcomeLabel.Text = $"Welcome {OperationParameters.currentUser}";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        UpdateStartStopImage();
        // Ensure welcome text and start background initialization when the page is visible
        welcomeLabel.Text = $"Welcome {OperationParameters.currentUser}";

        try
        {
            await StartUpCommandsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during startup commands: {ex.Message}");
        }
    }

    private void UpdateStartStopImage()
    {
        startStopButton.Source = OperationParameters.startClicked
            ? "stop_button.png"
            : "start_button.png";
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
            await DisplayAlertAsync("Error during location reading:", ex.Message, "OK");
        }
        connectionHelper = new();
        await connectionHelper.SendMyCurrentLocationAsync();
    }


    private async void backButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///EntryPage");
    }

    private async void editMyDataButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(UserInfoPage));
    }

    private async void startStopButton_Clicked(object sender, EventArgs e)
    {
        if (!_isLogging)
        {
            // start
            await _loggingService.StartAsync(OperationParameters.currentUser ?? string.Empty);
            _isLogging = true;
        }
        else
        {
            // stop
            await _loggingService.StopAsync();
            _isLogging = false;
        }
        if (OperationParameters.startClicked)
        { OperationParameters.startClicked = false; }
        else { OperationParameters.startClicked = true; }
        UpdateStartStopImage();


    }

    private async void logOffButton_Clicked(object sender, EventArgs e)
    {
        // stop logging if running
        if (_isLogging)
        {
            await _loggingService.StopAsync();
            _isLogging = false;
        }

        OperationParameters.currentUser = null;
        await Shell.Current.GoToAsync("///EntryPage");
    }

    private async void userDetailsButton_Clicked(object sender, EventArgs e)
    {
        // show user details page (ContactDetailsPage) by fetching the current user from DB
        connectionHelper = new();
        var me = await connectionHelper.GetUserByNickAsync(OperationParameters.currentUser ?? string.Empty);
        if (me != null)
        {
            OperationParameters.userInfo = me;
            OperationParameters.editEnabled = false;
            await Shell.Current.GoToAsync(nameof(UserInfoPage));
        }
        else
        {
            // Navigate to Edit page to view/edit (user may not have details yet)
            await Shell.Current.GoToAsync(nameof(UserInfoPage));
        }
    }
}