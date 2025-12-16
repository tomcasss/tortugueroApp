using tortugueroApp.Services;

namespace tortugueroApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;
    private bool _isPasswordVisible = false;

    public LoginPage()
    {
        InitializeComponent();
        _authService = new AuthService();
    }

    private void OnTogglePasswordVisibility(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        
        
        if (sender is ImageButton button)
        {
            button.Source = _isPasswordVisible ? "visibility.png" : "visibility_off.png";
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var usuarioOCorreo = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(usuarioOCorreo) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Please enter username/email and password", "OK");
            return;
        }

     
        LoginButton.IsEnabled = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var (success, message, usuario) = await _authService.LoginAsync(usuarioOCorreo, password);

            if (success)
            {
              
                EmailEntry.Text = string.Empty;
                PasswordEntry.Text = string.Empty;
                
                await DisplayAlert("Success", "Login successful", "OK");
                await Shell.Current.GoToAsync("//MainMenu");
            }
            else
            {
                await DisplayAlert("Error", message, "OK");
            }
        }
        finally
        {
            LoginButton.IsEnabled = true;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ForgotPassword");
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Register");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Welcome");
    }
}
