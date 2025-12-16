using tortugueroApp.Services;

namespace tortugueroApp.Views;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _authService;
    private bool _isPasswordVisible = false;
    private bool _isConfirmPasswordVisible = false;

    public RegisterPage()
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

    private void OnToggleConfirmPasswordVisibility(object sender, EventArgs e)
    {
        _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
        ConfirmPasswordEntry.IsPassword = !_isConfirmPasswordVisible;
        
        if (sender is ImageButton button)
        {
            button.Source = _isConfirmPasswordVisible ? "visibility.png" : "visibility_off.png";
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var fullName = FullNameEntry.Text?.Trim();
        var email = EmailEntry.Text?.Trim();
        var username = UsernameEntry.Text?.Trim();
        var phone = PhoneEntry.Text?.Trim();
        var password = PasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;

        // Validations
        if (string.IsNullOrWhiteSpace(fullName))
        {
            await DisplayAlert("Error", "Please enter your full name", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Error", "Please enter your email address", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            await DisplayAlert("Error", "Please enter a username", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Please enter a password", "OK");
            return;
        }

        if (password != confirmPassword)
        {
            await DisplayAlert("Error", "Passwords do not match", "OK");
            return;
        }

        if (password.Length < 6)
        {
            await DisplayAlert("Error", "Password must be at least 6 characters", "OK");
            return;
        }

        // Show loading
        RegisterButton.IsEnabled = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var (success, message, usuario) = await _authService.RegisterAsync(
                fullName, 
                email,
                username,
                password, 
                phone
            );

            if (success)
            {
                await DisplayAlert("Success", "Account created successfully", "OK");
                await Shell.Current.GoToAsync("//MainMenu");
            }
            else
            {
                await DisplayAlert("Error", message, "OK");
            }
        }
        finally
        {
            RegisterButton.IsEnabled = true;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Login");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Welcome");
    }
}
