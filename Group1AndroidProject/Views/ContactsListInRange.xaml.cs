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
    }
    private void checkIfNewUser()
    {
        isCurrentUserNew = connectionHelper.IsTheUserNew();
        if (isCurrentUserNew)
        {
           Shell.Current.GoToAsync(nameof(EditContactPage));
        }

    }

    private void welcomeLabel_Loaded(object sender, EventArgs e)
    {
        welcomeLabel.Text = $"Welcome dear {OperationParameters.currentUser}";
    }

    private void ContentPage_Appearing(object sender, EventArgs e)
    {
        checkIfNewUser();

    }
}