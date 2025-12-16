using tortugueroApp.Models;
using tortugueroApp.Services;
using System.Security.Cryptography;
using System.Text;

namespace tortugueroApp.Views;

public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage()
    {
        InitializeComponent();
    }

    private void OnToggleNewPasswordVisibility(object sender, EventArgs e)
    {
        NewPasswordEntry.IsPassword = !NewPasswordEntry.IsPassword;
    }

    private void OnToggleConfirmPasswordVisibility(object sender, EventArgs e)
    {
        ConfirmPasswordEntry.IsPassword = !ConfirmPasswordEntry.IsPassword;
    }

    private async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var newPassword = NewPasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;

        if (string.IsNullOrEmpty(email))
        {
            await DisplayAlert("Error", "Please enter your email", "OK");
            return;
        }

        if (string.IsNullOrEmpty(newPassword))
        {
            await DisplayAlert("Error", "Please enter a new password", "OK");
            return;
        }

        if (newPassword != confirmPassword)
        {
            await DisplayAlert("Error", "Passwords do not match", "OK");
            return;
        }

        if (newPassword.Length < 6)
        {
            await DisplayAlert("Error", "Password must be at least 6 characters", "OK");
            return;
        }

        try
        {
            ResetButton.IsEnabled = false;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var client = await SupabaseService.GetClientAsync();
            
            var userResponse = await client
                .From<Usuario>()
                .Where(x => x.Correo == email)
                .Get();

            if (userResponse.Models.Count == 0)
            {
                await DisplayAlert("Error", "No account found with this email", "OK");
                return;
            }

            var user = userResponse.Models[0];
            var hashedPassword = HashPassword(newPassword);

            var updatedUser = new Usuario
            {
                IdUsuario = user.IdUsuario,
                NombreCompleto = user.NombreCompleto,
                NombreUsuario = user.NombreUsuario,
                Correo = user.Correo,
                ContraseniaHash = hashedPassword,
                NumeroTelefono = user.NumeroTelefono,
                Edad = user.Edad,
                Domicilio = user.Domicilio,
                FechaCreacion = user.FechaCreacion
            };

            await client.From<Usuario>()
                .Where(x => x.IdUsuario == user.IdUsuario)
                .Update(updatedUser);

            await DisplayAlert("Success", "Your password has been reset successfully", "OK");
            await Shell.Current.GoToAsync("//Login");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not reset password: {ex.Message}", "OK");
        }
        finally
        {
            ResetButton.IsEnabled = true;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Login");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Login");
    }
}
