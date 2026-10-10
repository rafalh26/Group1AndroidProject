using ProjektGrupowy.Models;
using ProjektGrupowy.Services;

namespace ProjektGrupowy.Views;

public partial class UserInfoPage : ContentPage
{
    public ConnectionHelper connectionHelper { get; set; }

    private User user = new();

    public UserInfoPage()
    {
        InitializeComponent();
        connectionHelper = new ConnectionHelper();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await ConfigurePageAsync();
    }

    private async Task ConfigurePageAsync()
    {
        bool isRegistration = OperationParameters.newUser;
        bool isEditing = !isRegistration && OperationParameters.editEnabled;
        bool isViewing = !isRegistration && !isEditing;

        // Show the intro labels only during registration (use FindByName to avoid generated field dependency)
        var firstLbl = this.FindByName<Label>("firstTimeLabel");
        if (firstLbl != null) firstLbl.IsVisible = isRegistration;
        var firstDesc = this.FindByName<Label>("firstTimeDescLabel");
        if (firstDesc != null) firstDesc.IsVisible = isRegistration;

        passwordEntry.IsVisible = isRegistration || isEditing;
        passwordEntry.Text = string.Empty;

        nameEntry.IsReadOnly = isViewing;
        emailEntry.IsReadOnly = isViewing;
        phoneEntry.IsReadOnly = isViewing;
        addressEntry.IsReadOnly = isViewing;

        saveButton.IsVisible = !isViewing;

        // When in view-only mode we don't want to show the input placeholders (they look like default text).
        if (isViewing)
        {
            nameEntry.Placeholder = string.Empty;
            emailEntry.Placeholder = string.Empty;
            phoneEntry.Placeholder = string.Empty;
            addressEntry.Placeholder = string.Empty;
            passwordEntry.Placeholder = string.Empty;
        }

        if (isRegistration)
        {
            user = new User
            {
                nick = OperationParameters.currentUser ?? string.Empty,
                pass_hash = string.Empty
            };

            nameEntry.Text = string.Empty;
            emailEntry.Text = string.Empty;
            phoneEntry.Text = string.Empty;
            addressEntry.Text = string.Empty;

            pageTitle.Text = "Welcome!";
            pageDescription.Text = "Let's set up your profile.";

            passwordEntry.Placeholder = "Create a password (required)";
            saveButton.Text = "Create account";

            return;
        }

        // Fetch fresh information for edit and view modes.
        connectionHelper = new ConnectionHelper();

        var loadedUser = await connectionHelper.GetUserByNickAsync(
            OperationParameters.currentUser ?? string.Empty);

        if (loadedUser == null)
        {
            await DisplayAlert(
                "Error",
                "Could not load your user information.",
                "OK");

            await Shell.Current.GoToAsync("..");
            return;
        }

        user = loadedUser;
        OperationParameters.userInfo = loadedUser;

        nameEntry.Text = user.name ?? string.Empty;
        emailEntry.Text = user.email ?? string.Empty;
        phoneEntry.Text = user.phone ?? string.Empty;
        addressEntry.Text = user.address ?? string.Empty;

        if (isEditing)
        {
            pageTitle.Text = "Edit your profile";
            pageDescription.Text = "Update your information below.";

            passwordEntry.Placeholder =
                "New password (leave blank to keep current)";

            saveButton.Text = "Save changes";
        }
        else
        {
            pageTitle.Text = "Your details";
            pageDescription.Text = "Your account information.";
        }
    }

    private async void saveButton_Clicked(object sender, EventArgs e)
    {
        bool isRegistration = OperationParameters.newUser;
        bool isEditing = !isRegistration && OperationParameters.editEnabled;

        // This page must not save data in view-only mode.
        if (!isRegistration && !isEditing)
            return;

        string name = nameEntry.Text ?? string.Empty;
        string password = passwordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert(
                "Required field",
                "Please provide your name.",
                "OK");

            return;
        }

        if (isRegistration && string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert(
                "Required field",
                "Please create a password.",
                "OK");

            return;
        }

        user.name = name;
        user.email = emailEntry.Text;
        user.phone = phoneEntry.Text;
        user.address = addressEntry.Text;

        try
        {
            if (isRegistration)
            {
                await connectionHelper.RegisterNewUserAsync(user, password);

                OperationParameters.newUser = false;
                OperationParameters.editEnabled = false;
            }
            else
            {
                // An empty password means keep the existing password.
                await connectionHelper.UpdateCurrentContactInformation(
                    user,
                    string.IsNullOrWhiteSpace(password) ? null : password);

                OperationParameters.editEnabled = false;
            }

            // Keep the shared in-memory user information up to date.
            OperationParameters.userInfo = user;

            await DisplayAlert(
                "Success",
                isRegistration
                    ? "Your account has been created."
                    : "Your changes have been saved.",
                "OK");

            await Shell.Current.GoToAsync(nameof(MainPage));
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error saving data",
                ex.Message,
                "OK");

            Console.WriteLine($"Error saving user information: {ex}");
        }
    }

    private async void backButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void quitButton_Clicked(object sender, EventArgs e)
    {
        var logger = LoggingService.Instance;
        if (logger != null && logger.IsRunning)
        {
            try
            {
                await logger.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping logger before quit: {ex.Message}");
            }
        }

        try
        {
            Application.Current.Quit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to quit app: {ex.Message}");
        }
    }
}