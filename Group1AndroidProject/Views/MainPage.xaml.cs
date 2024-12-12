using Group1AndroidProject.Models;

namespace Group1AndroidProject.Views;

public partial class MainPage : ContentPage
{

    ConnectionHelper connectionHelper;
    //public ObservableCollection<Models.Contact> ContactsInRange { get; set; }

    public MainPage()
	{
		InitializeComponent();
        connectionHelper = new ConnectionHelper();



        StartUpCommandsAsync();
    }
    public async void StartUpCommandsAsync()
    {
        await GetCurrentLocation();
        GetTheData();
        ProcessTheData();
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
        connectionHelper = new();
        await connectionHelper.SendMyCurrentLocationAsync();
    }
    public void GetTheData()
    {
        connectionHelper = new();
        connectionHelper.GatherSourceData();
    }
    public void ProcessTheData()
    {

        //DISTANCE
        if (OperationParameters.gatheringDataCompleted)
        {
            OperationParameters.contactsInRange = new();
            foreach (var contact in OperationParameters.contactsList)
            {
                if (OperationParameters.MyCurrentLocation != null)
                {
                    contact.distanceFromCurrentUserinMeters = GeoUtils.CalculateDistance(
                        OperationParameters.MyCurrentLocation.Latitude,
                        OperationParameters.MyCurrentLocation.Longitude,
                        contact.geo_latitude,
                        contact.geo_longitude);

                    if (contact.distanceFromCurrentUserinMeters < 500000000) // 50 km range
                    {
                        contact.directionFromCurrentUser = GeoUtils.CalculateBearing(
                            OperationParameters.MyCurrentLocation.Latitude,
                            OperationParameters.MyCurrentLocation.Longitude,
                            contact.geo_latitude,
                            contact.geo_longitude);

                        OperationParameters.contactsInRange.Add(contact);

                        Console.WriteLine($"Contact in range: {contact.nick}, Distance: {contact.distanceFromCurrentUserinMeters} m, Direction: {contact.directionFromCurrentUser:F2}°");
                    }
                }
            }
        }
        if (OperationParameters.contactsInRange != null)
        {
            contactsToDisplay.ItemsSource = OperationParameters.contactsInRange;
        }
    }

    private async void contactsToDisplay_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Models.Contact selectedContact)
        {
            OperationParameters.contactDetails = selectedContact;
        }
        await Shell.Current.GoToAsync(nameof(ContactDetailsPage));
    }

    private void contactsToDisplay_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        contactsToDisplay.SelectedItem = null;
    }
}