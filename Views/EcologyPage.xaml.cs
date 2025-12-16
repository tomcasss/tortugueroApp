namespace tortugueroApp.Views;

public partial class EcologyPage : ContentPage
{
    public EcologyPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainMenu");
    }
}
