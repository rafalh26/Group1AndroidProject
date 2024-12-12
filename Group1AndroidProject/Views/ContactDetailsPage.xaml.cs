using Group1AndroidProject.Models;

namespace Group1AndroidProject.Views;

public partial class ContactDetailsPage : ContentPage
{
	public ContactDetailsPage()
	{
		InitializeComponent();
        BindingContext = OperationParameters.contactDetails;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MainPage));
    }
}