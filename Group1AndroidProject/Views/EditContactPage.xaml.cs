using Group1AndroidProject.Models;
using Microsoft.Maui.Platform;
using System.Reflection.Emit;

namespace Group1AndroidProject.Views;

public partial class EditContactPage : ContentPage
{
    public ConnectionHelper connectionHelper { get; set; }

    public EditContactPage()
	{
        InitializeComponent();
	}

    private async void saveButton_Clicked(object sender, EventArgs e)
    {
        connectionHelper = new ConnectionHelper();
        if (string.IsNullOrEmpty(nameEntry.Text) || string.IsNullOrEmpty(emailEntry.Text))
        {
            await DisplayAlert("Required fields error!", "Hey dude are U sure that You are nameless?\n Did You also forgot your internet personality existance by not sharing Your email address with us?\n Play nicely and provide required data","Yhmmm..");
        }
        else
        {
            await connectionHelper.UpdateCurrentContactInformation(nameEntry.Text, emailEntry.Text);
            await Shell.Current.GoToAsync(nameof(MainPage));
        }
    }
    private async void backButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///EntryPage");
    }
}