using Group1AndroidProject.Models;

namespace Group1AndroidProject.Views;

public partial class ContactDetailsPage : ContentPage
{
	public ContactDetailsPage()
	{
		InitializeComponent();
        BindingContext = OperationParameters.contactDetails;
    }
}