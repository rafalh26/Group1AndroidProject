using ProjektGrupowy.Services;

namespace ProjektGrupowy.Views;

public partial class ChangeNickPage : ContentPage
{
    ConnectionHelper connectionHelper;

    public ChangeNickPage()
    {
        InitializeComponent();
        connectionHelper = new ConnectionHelper();
    }

    private async void cancelButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///MainPage");
    }

    private async void saveButton_Clicked(object sender, EventArgs e)
    {
        var oldNick = OperationParameters.currentUser;
        var currentPassword = currentPasswordEntry.Text ?? string.Empty;
        var newNick = newNickEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(oldNick))
        {
            await DisplayAlertAsync("Error", "No current user is logged in.", "OK");
            return;
        }

        // Verify current password before allowing nick change
        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            await DisplayAlertAsync("Error", "Please enter your current password to confirm nick change.", "OK");
            return;
        }

        var storedHash = await connectionHelper.GetPassHashAsync(oldNick);
        if (string.IsNullOrWhiteSpace(storedHash))
        {
            // no password set for this account
            await DisplayAlertAsync("Error", "No password is set for the current account. Please set a password first.", "OK");
            await Shell.Current.GoToAsync(nameof(EditContactPage));
            return;
        }

        var providedHash = ConnectionHelper.ComputeHash(currentPassword);
        if (providedHash != storedHash)
        {
            await DisplayAlertAsync("Error", "Incorrect password.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(newNick))
        {
            await DisplayAlertAsync("Error", "Please enter a valid new nick.", "OK");
            return;
        }

        if (newNick == oldNick)
        {
            await DisplayAlertAsync("Info", "New nick is the same as current.", "OK");
            return;
        }

        // create new contact (if DB available) by setting currentUser and calling SendEnterQueryAsync
        OperationParameters.currentUser = newNick;
        try
        {
            await connectionHelper.SendEnterQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: could not insert new nick: {ex.Message}");
        }

        // delete old nick related records
        if (!string.IsNullOrWhiteSpace(oldNick))
        {
            try
            {
                await connectionHelper.DeleteUserRecordsByNickAsync(oldNick);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting old nick records: {ex.Message}");
            }
        }

        // navigate to edit page so user can fill details/password for new nick
        await Shell.Current.GoToAsync(nameof(EditContactPage));
    }
}
