using tortugueroApp.Services;

namespace tortugueroApp.Views;

public partial class WelcomePage : ContentPage
{
    private readonly SessionService _sessionService;

    public WelcomePage()
    {
        InitializeComponent();
        _sessionService = SessionService.Instance;
    }

    private async void OnExploreHotelClicked(object sender, EventArgs e)
    {
        
        _sessionService.SetGuestMode();
        
      
        await Shell.Current.GoToAsync("//MainMenu");
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Login");
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Register");
    }
}
