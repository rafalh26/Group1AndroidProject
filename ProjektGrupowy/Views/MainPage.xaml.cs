using ProjektGrupowy.Services;

namespace ProjektGrupowy.Views;

public partial class MainPage : ContentPage
{

    ConnectionHelper? connectionHelper;
    LoggingService _loggingService = new();
    private bool _isLogging = false;
    private int? _currentTrackId = null;
    //public ObservableCollection<Models.User> ContactsInRange { get; set; }

/*    public MainPage()
	{
		InitializeComponent();
        connectionHelper = new ConnectionHelper();



        StartUpCommandsAsync();
    }*/
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
        await Shell.Current.GoToAsync(nameof(EditContactPage));
    }

    private async void startStopButton_Clicked(object sender, EventArgs e)
    {
        if (!_isLogging)
        {
            // start
            await _loggingService.StartAsync(OperationParameters.currentUser ?? string.Empty);
            _isLogging = true;
            startStopButton.Text = "Stop";
        }
        else
        {
            // stop
            await _loggingService.StopAsync();
            _isLogging = false;
            startStopButton.Text = "Start";
        }
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
            await Shell.Current.GoToAsync(nameof(ContactDetailsPage));
        }
        else
        {
            // Navigate to Edit page to view/edit (user may not have details yet)
            await Shell.Current.GoToAsync(nameof(EditContactPage));
        }
    }

    private async void changeNickButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ChangeNickPage));
    }
}