using tortugueroApp.Services;
using tortugueroApp.Models;

namespace tortugueroApp.Views;

public partial class ProfilePage : ContentPage
{
    private readonly SessionService _sessionService;

    public ProfilePage()
    {
        InitializeComponent();
        _sessionService = SessionService.Instance;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadUserProfile();
    }

    private void LoadUserProfile()
    {
        if (_sessionService.IsLoggedIn && _sessionService.CurrentUser != null)
        {
           
            var user = _sessionService.CurrentUser;

         
            NameLabel.Text = user.NombreCompleto ?? "User";
            EmailLabel.Text = user.Correo ?? "";
            UsernameLabel.Text = user.NombreUsuario ?? "-";
            PhoneLabel.Text = user.NumeroTelefono ?? "Not registered";
            
       
            if (user.FechaCreacion.HasValue)
            {
                MemberSinceLabel.Text = user.FechaCreacion.Value.ToString("MMMM yyyy");
            }
            else
            {
                MemberSinceLabel.Text = "-";
            }

         
            AvatarLabel.Text = "👤";

           
            UserInfoSection.IsVisible = true;
            LoggedInActions.IsVisible = true;
            GuestActions.IsVisible = false;
        }
        else
        {
            
            NameLabel.Text = "Guest";
            EmailLabel.Text = "Explore Tortuguero as a guest";
            AvatarLabel.Text = "👤";

          
            UserInfoSection.IsVisible = false;
            LoggedInActions.IsVisible = false;
            GuestActions.IsVisible = true;
        }
    }

    private bool _isEditMode = false;

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        if (!_isEditMode)
        {
            _isEditMode = true;
            EditButton.Text = "Cancel";
            SaveButton.IsVisible = true;
            
            NameEntry.IsVisible = true;
            NameLabel.IsVisible = false;
            NameEntry.Text = _sessionService.CurrentUser?.NombreCompleto ?? "";

            UsernameEntry.IsVisible = true;
            UsernameLabel.IsVisible = false;
            UsernameEntry.Text = _sessionService.CurrentUser?.NombreUsuario ?? "";

            PhoneEntry.IsVisible = true;
            PhoneLabel.IsVisible = false;
            PhoneEntry.Text = _sessionService.CurrentUser?.NumeroTelefono ?? "";
        }
        else
        {
            _isEditMode = false;
            EditButton.Text = "Edit Profile";
            SaveButton.IsVisible = false;

            NameEntry.IsVisible = false;
            NameLabel.IsVisible = true;

            UsernameEntry.IsVisible = false;
            UsernameLabel.IsVisible = true;

            PhoneEntry.IsVisible = false;
            PhoneLabel.IsVisible = true;
        }
    }

    private async void OnSaveProfileClicked(object sender, EventArgs e)
    {
        try
        {
            var name = NameEntry.Text?.Trim();
            var username = UsernameEntry.Text?.Trim();
            var phone = PhoneEntry.Text?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                await DisplayAlert("Error", "Name is required", "OK");
                return;
            }

            if (string.IsNullOrEmpty(username))
            {
                await DisplayAlert("Error", "Username is required", "OK");
                return;
            }

            var client = await SupabaseService.GetClientAsync();
            var userId = _sessionService.CurrentUser!.IdUsuario;

            var updatedUser = new Usuario
            {
                IdUsuario = userId,
                NombreCompleto = name,
                NombreUsuario = username,
                NumeroTelefono = phone,
                Correo = _sessionService.CurrentUser.Correo,
                ContraseniaHash = _sessionService.CurrentUser.ContraseniaHash,
                Edad = _sessionService.CurrentUser.Edad,
                Domicilio = _sessionService.CurrentUser.Domicilio,
                FechaCreacion = _sessionService.CurrentUser.FechaCreacion
            };

            await client.From<Usuario>()
                .Where(x => x.IdUsuario == userId)
                .Update(updatedUser);

            _sessionService.CurrentUser.NombreCompleto = name;
            _sessionService.CurrentUser.NombreUsuario = username;
            _sessionService.CurrentUser.NumeroTelefono = phone;

            _isEditMode = false;
            EditButton.Text = "Edit Profile";
            SaveButton.IsVisible = false;

            LoadUserProfile();

            await DisplayAlert("Success", "Profile updated successfully", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not update profile: {ex.Message}", "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Logout",
            "Are you sure you want to logout?",
            "Yes",
            "No");

        if (confirm)
        {
            _sessionService.Logout();
            await Shell.Current.GoToAsync("//Welcome");
        }
    }

    private async void OnBackToMenuClicked(object sender, EventArgs e)
    {
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

    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Welcome");
    }
}
