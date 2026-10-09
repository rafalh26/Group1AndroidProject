using ProjektGrupowy.Services;

namespace ProjektGrupowy.Views;

public partial class ContactDetailsPage : ContentPage
{
	public ContactDetailsPage()
	{
		InitializeComponent();
        BindingContext = OperationParameters.userInfo;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MainPage));
    }
}