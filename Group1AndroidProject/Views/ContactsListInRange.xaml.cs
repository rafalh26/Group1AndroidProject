using Group1AndroidProject.ConnectSQL;
namespace Group1AndroidProject.Views;

public partial class ContactsListInRange : ContentPage
{

    public ContactsListInRange()
	{
        InitializeComponent();
	}


    private void welcomeLabel_Loaded(object sender, EventArgs e)
    {
        var label = sender as Label;

        label.Text = $"Welcome dear ...";
    }
}