using tortugueroApp.Services;

namespace tortugueroApp.Views;

public partial class TortugueroMainPage : ContentPage
{
    private readonly SessionService _sessionService;

    public TortugueroMainPage()
    {
        InitializeComponent();
        _sessionService = SessionService.Instance;
        AnimatePageLoad();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateUIForSession();
        UpdateCartBadge();
        
        CartService.Instance.OnCartChanged += OnCartChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        CartService.Instance.OnCartChanged -= OnCartChanged;
    }

    private void OnCartChanged()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateCartBadge();
        });
    }

    private void UpdateUIForSession()
    {
        if (_sessionService.IsLoggedIn && _sessionService.CurrentUser != null)
        {
            var nombre = _sessionService.CurrentUser.NombreCompleto;
            if (!string.IsNullOrEmpty(nombre))
            {
                var primerNombre = nombre.Split(' ')[0];
                UserNameLabel.Text = primerNombre.Length > 15 ? primerNombre.Substring(0, 15) + "..." : primerNombre;
            }
            else
            {
                UserNameLabel.Text = "Usuario";
            }
        }
        else if (_sessionService.IsGuestMode)
        {
            UserNameLabel.Text = "Invitado";
        }
        else
        {
            UserNameLabel.Text = "Invitado";
        }
    }

    private async void AnimatePageLoad()
    {
        var heroFrame = this.FindByName<View>("HeroFrame");
        var menuGrid = this.FindByName<Grid>("MenuGrid");

        if (heroFrame != null)
        {
            heroFrame.Opacity = 0;
            heroFrame.TranslationY = -50;
            await heroFrame.FadeTo(1, 800, Easing.CubicOut);
            await heroFrame.TranslateTo(0, 0, 600, Easing.CubicOut);
        }

        if (menuGrid != null)
        {
            menuGrid.Opacity = 0;
            menuGrid.TranslationY = 50;
            await Task.Delay(200);
            await menuGrid.FadeTo(1, 800, Easing.CubicOut);
            await menuGrid.TranslateTo(0, 0, 600, Easing.CubicOut);
        }
    }

    private async Task AnimateButton(View view)
    {
        await view.ScaleTo(0.95, 100, Easing.CubicOut);
        await view.ScaleTo(1.0, 100, Easing.CubicOut);
    }

    private async void OnMenuClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Menú", "Menú de navegación", "OK");
    }

    private async void OnAccountClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Profile");
    }

    private async void OnGeographyTapped(object sender, EventArgs e)
    {
        if (sender is View view)
            await AnimateButton(view);
        await Shell.Current.GoToAsync("//Geography");
    }

    private async void OnEcologyTapped(object sender, EventArgs e)
    {
        if (sender is View view)
            await AnimateButton(view);
        await Shell.Current.GoToAsync("//Ecology");
    }

    private async void OnMapTapped(object sender, EventArgs e)
    {
        if (sender is View view)
            await AnimateButton(view);
        await Shell.Current.GoToAsync(nameof(MapPage));
    }

    private async void OnHistoryTapped(object sender, EventArgs e)
    {
        if (sender is View view)
            await AnimateButton(view);
        await Shell.Current.GoToAsync("//History");
    }

    private async void OnHelpTapped(object sender, EventArgs e)
    {
        if (sender is View view)
            await AnimateButton(view);
        await Shell.Current.GoToAsync("//Help");
    }

    private async void OnBookNowTapped(object sender, EventArgs e)
    {
        if (sender is View view)
            await AnimateButton(view);
        
        await Shell.Current.GoToAsync("//Booking");
    }

    private void OnHomeTapped(object sender, EventArgs e)
    {
    }

    private async void OnActivitiesTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Activities");
    }

    private async void OnCartTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Cart");
    }

    private async void OnMyBookingsTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MyBookings");
    }

    private async void OnProfileTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Profile");
    }

    private void UpdateCartBadge()
    {
        var cartService = CartService.Instance;
        var totalItems = cartService.TotalItems;

        if (totalItems > 0)
        {
            CartBadge.IsVisible = true;
            CartBadgeLabel.Text = totalItems > 9 ? "9+" : totalItems.ToString();
        }
        else
        {
            CartBadge.IsVisible = false;
        }
    }
}
