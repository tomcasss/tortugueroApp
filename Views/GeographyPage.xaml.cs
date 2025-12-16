namespace tortugueroApp.Views;

public partial class GeographyPage : ContentPage
{
    public GeographyPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainMenu");
    }
}
