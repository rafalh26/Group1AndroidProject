using ProjektGrupowy.Models;
using ProjektGrupowy.Services;

namespace ProjektGrupowy.Views;

public partial class EditContactPage : ContentPage
{
    public ConnectionHelper connectionHelper { get; set; }
    User user;
    public EditContactPage()
	{
        InitializeComponent();
        connectionHelper = new ConnectionHelper();

        user = new User
        {
            nick = OperationParameters.currentUser ?? string.Empty,
        };

        // If arriving here for registration, show password entry
        passwordEntry.IsVisible = OperationParameters.newUser;
	}



    private async void saveButton_Clicked(object sender, EventArgs e)
    {
        user.name = nameEntry.Text;
        user.email = emailEntry.Text;
        user.phone = phoneEntry.Text;
        user.address = addressEntry.Text;





        connectionHelper = new ConnectionHelper();
        // Validate required fields depending on whether this is a registration or an edit
        if (string.IsNullOrEmpty(user.name) || (OperationParameters.newUser && string.IsNullOrEmpty(passwordEntry.Text)))
        {
            await DisplayAlertAsync("Required fields error!", "Please provide: name. New users must also set a password.", "OK");
            return;
        }

        try
        {
            if (OperationParameters.newUser)
            {
                // Register new user: save name,email and password hash (upsert)
                await connectionHelper.RegisterNewUserAsync(user, passwordEntry.Text);
                OperationParameters.newUser = false;
                await DisplayAlert("Success", "Registration saved.", "OK");
                await Shell.Current.GoToAsync(nameof(MainPage));
            }
            else
            {
                await connectionHelper.UpdateCurrentContactInformation(user.name, user.email);
                await DisplayAlert("Success", "Your changes were saved.", "OK");
                await Shell.Current.GoToAsync(nameof(MainPage));
            }
        }
        catch (Exception ex)
        {
            // Show the error to the user for troubleshooting
            var msg = ex.Message;
            if (!string.IsNullOrEmpty(OperationParameters.errorDisplayer))
                msg += "\n" + OperationParameters.errorDisplayer;
            await DisplayAlert("Error saving data", msg, "OK");
            Console.WriteLine($"Error in saveButton_Clicked: {ex}");
        }
    }
    private async void backButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///EntryPage");
    }
}