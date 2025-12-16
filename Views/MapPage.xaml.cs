using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace tortugueroApp.Views;

public partial class MapPage : ContentPage
{
    private readonly Location pachiraLodgeLocation = new Location(10.548678705912478, -83.51008415230898);
    private bool isHybridView = true;

    public MapPage()
    {
        InitializeComponent();
        InitializeMap();
        RequestLocationPermissions();
    }

    private async void RequestLocationPermissions()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error requesting location permissions: {ex.Message}");
        }
    }

    private void InitializeMap()
    {
        var pin = new Pin
        {
            Label = "Pachira Lodge",
            Address = "Tortuguero, Costa Rica",
            Type = PinType.Place,
            Location = pachiraLodgeLocation
        };

        TortugueroMap.Pins.Add(pin);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        System.Diagnostics.Debug.WriteLine($"MapPage OnAppearing - Pachira Lodge Location: {pachiraLodgeLocation.Latitude}, {pachiraLodgeLocation.Longitude}");
        
        await Task.Delay(100);
        CenterMapOnLodge();
        
        await Task.Delay(1000);
        CenterMapOnLodge();
    }

    private void CenterMapOnLodge()
    {
        System.Diagnostics.Debug.WriteLine($"CenterMapOnLodge - Moving to: {pachiraLodgeLocation.Latitude}, {pachiraLodgeLocation.Longitude}");
        
        var mapSpan = MapSpan.FromCenterAndRadius(
            pachiraLodgeLocation,
            Distance.FromKilometers(0.5)
        );

        TortugueroMap.MoveToRegion(mapSpan);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainMenu");
    }

    private void OnMapTypeToggled(object sender, EventArgs e)
    {
        if (isHybridView)
        {
            TortugueroMap.MapType = MapType.Street;
            isHybridView = false;
        }
        else
        {
            TortugueroMap.MapType = MapType.Hybrid;
            isHybridView = true;
        }
    }

    private void OnCenterLocation(object sender, EventArgs e)
    {
        CenterMapOnLodge();
    }

    private async void OnNavigateToLodge(object sender, EventArgs e)
    {
        try
        {
            var location = new Location(pachiraLodgeLocation.Latitude, pachiraLodgeLocation.Longitude);
            var options = new MapLaunchOptions { Name = "Pachira Lodge", NavigationMode = NavigationMode.Driving };
            
            await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Navigation", 
                "Could not open navigation. Make sure you have a maps app installed.", 
                "OK");
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }
}
