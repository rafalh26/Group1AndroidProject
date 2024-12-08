using Group1AndroidProject.Parameters;
using Group1AndroidProject.ConnectSQL;
namespace Group1AndroidProject.Views;

public partial class ContactsListInRange : ContentPage
{
    ConnectionHelper connectionHelper;
    bool isCurrentUserNew=false;

    public ContactsListInRange()
    {
        InitializeComponent();
        connectionHelper = new ConnectionHelper();
        InitializeAsync();
    }
    public async void InitializeAsync()
    {
        // Await the asynchronous operation to complete
        isCurrentUserNew = await connectionHelper.IsTheUserNewAsync();

        // Now that isCurrentUserNew has been updated, check the condition
        if (isCurrentUserNew)
        {
            // Navigate to the EditContactPage
            _ = Shell.Current.GoToAsync(nameof(EditContactPage));
        }
    }

    private async void isCurrentUserNewCheck()
    {
        isCurrentUserNew = await connectionHelper.IsTheUserNewAsync();
    }

    private void welcomeLabel_Loaded(object sender, EventArgs e)
    {
        var label = sender as Label;

        label.Text = $"Welcome dear {OperationParameters.currentUser}";
    }
        
}