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
        checkIfNewUser();
    }
    private async void checkIfNewUser()
    {
        isCurrentUserNew = await connectionHelper.IsTheUserNewAsync();
        if (isCurrentUserNew)
        {
            _ = Shell.Current.GoToAsync(nameof(EditContactPage));
        }

    }

    private void welcomeLabel_Loaded(object sender, EventArgs e)
    {
        var label = sender as Label;

        label.Text = $"Welcome dear {OperationParameters.currentUser}";

    }

}