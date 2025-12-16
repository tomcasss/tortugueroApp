using tortugueroApp.Models;
using tortugueroApp.Services;

namespace tortugueroApp.Views;

public partial class BookingPage : ContentPage
{
    private List<Habitacion> _habitaciones = new();
    private int _guestCount = 1;
    private DateTime _checkInDate;
    private DateTime _checkOutDate;
    private int _numberOfNights = 0;
    private readonly CartService _cartService;
    private readonly SessionService _sessionService;

    public BookingPage()
    {
        InitializeComponent();
        _cartService = CartService.Instance;
        _sessionService = SessionService.Instance;

        _checkInDate = DateTime.Today.AddDays(1);
        _checkOutDate = DateTime.Today.AddDays(2);
        CheckInDatePicker.Date = _checkInDate;
        CheckOutDatePicker.Date = _checkOutDate;
        CheckInDatePicker.MinimumDate = DateTime.Today;
        CheckOutDatePicker.MinimumDate = DateTime.Today.AddDays(1);

        UpdateNightsDisplay();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRoomsAsync();
    }

    private async Task LoadRoomsAsync()
    {
        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            RoomsContainer.Clear();
            EmptyStateView.IsVisible = false;

            var client = await SupabaseService.GetClientAsync();
            var response = await client
                .From<Habitacion>()
                .Where(x => x.Disponible == true)
                .Get();

            _habitaciones = response.Models.ToList();

            if (_habitaciones.Count == 0)
            {
                EmptyStateView.IsVisible = true;
            }
            else
            {
                foreach (var habitacion in _habitaciones)
                {
                    var roomCard = CreateRoomCard(habitacion);
                    RoomsContainer.Add(roomCard);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load rooms: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private Frame CreateRoomCard(Habitacion habitacion)
    {
        var card = new Frame
        {
            Padding = 0,
            CornerRadius = 12,
            HasShadow = true,
            BorderColor = Colors.Transparent,
            BackgroundColor = Colors.White,
            IsClippedToBounds = true,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var mainLayout = new VerticalStackLayout { Spacing = 0 };

        
        if (!string.IsNullOrEmpty(habitacion.ImagenUrl))
        {
            var imageFrame = new Frame
            {
                Padding = 0,
                CornerRadius = 0,
                HasShadow = false,
                BorderColor = Colors.Transparent,
                IsClippedToBounds = true,
                HeightRequest = 180
            };

            var image = new Image
            {
                Source = habitacion.ImagenUrl,
                Aspect = Aspect.AspectFill
            };

            imageFrame.Content = image;
            mainLayout.Add(imageFrame);
        }

    
        var contentLayout = new VerticalStackLayout
        {
            Padding = 15,
            Spacing = 8
        };

       
        var nameLayout = new HorizontalStackLayout { Spacing = 8 };
        nameLayout.Add(new Label
        {
            Text = habitacion.Nombre,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#2E4E49")
        });
        if (!string.IsNullOrEmpty(habitacion.Tipo))
        {
            nameLayout.Add(new Frame
            {
                Padding = new Thickness(8, 4),
                CornerRadius = 8,
                HasShadow = false,
                BackgroundColor = Color.FromArgb("#E0F2F1"),
                Content = new Label
                {
                    Text = habitacion.Tipo,
                    FontSize = 12,
                    TextColor = Color.FromArgb("#11d4b4")
                }
            });
        }
        contentLayout.Add(nameLayout);

       
        if (!string.IsNullOrEmpty(habitacion.Descripcion))
        {
            contentLayout.Add(new Label
            {
                Text = habitacion.Descripcion,
                FontSize = 14,
                TextColor = Color.FromArgb("#6B7280"),
                LineHeight = 1.3
            });
        }

     
        var capacityLayout = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(0, 5, 0, 0) };
        capacityLayout.Add(new Label { Text = "👥", FontSize = 16 });
        capacityLayout.Add(new Label
        {
            Text = $"Up to {habitacion.CapacidadPersonas} guests",
            FontSize = 14,
            TextColor = Color.FromArgb("#6B7280"),
            VerticalOptions = LayoutOptions.Center
        });
        contentLayout.Add(capacityLayout);

   
        if (habitacion.Amenidades != null && habitacion.Amenidades.Count > 0)
        {
            var amenitiesLayout = new FlexLayout
            {
                Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
                Margin = new Thickness(0, 8, 0, 0)
            };

            foreach (var amenidad in habitacion.Amenidades.Take(4))
            {
                var amenityChip = new Frame
                {
                    Padding = new Thickness(10, 5),
                    CornerRadius = 12,
                    HasShadow = false,
                    BackgroundColor = Color.FromArgb("#F3F4F6"),
                    Margin = new Thickness(0, 0, 8, 8),
                    Content = new Label
                    {
                        Text = amenidad,
                        FontSize = 12,
                        TextColor = Color.FromArgb("#6B7280")
                    }
                };
                amenitiesLayout.Add(amenityChip);
            }

            contentLayout.Add(amenitiesLayout);
        }

    
        var priceButtonGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Margin = new Thickness(0, 12, 0, 0)
        };

        var priceLayout = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center };
        var priceLabel = new Label
        {
            Text = $"${habitacion.PrecioPorNoche:F0}",
            FontSize = 26,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#11d4b4")
        };
        var nightLabel = new Label
        {
            Text = _numberOfNights > 0 ? $"per night • {_numberOfNights} nights = ${habitacion.PrecioPorNoche * _numberOfNights:F0}" : "per night",
            FontSize = 12,
            TextColor = Color.FromArgb("#6B7280")
        };
        priceLayout.Add(priceLabel);
        priceLayout.Add(nightLabel);

        var addButton = new Button
        {
            Text = "Add to Cart",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#11d4b4"),
            CornerRadius = 10,
            Padding = new Thickness(20, 10),
            VerticalOptions = LayoutOptions.Center
        };
        addButton.Clicked += async (s, e) => await OnAddRoomToCartClicked(habitacion);

        priceButtonGrid.Add(priceLayout, 0, 0);
        priceButtonGrid.Add(addButton, 1, 0);
        contentLayout.Add(priceButtonGrid);

        mainLayout.Add(contentLayout);
        card.Content = mainLayout;

        return card;
    }

    private async Task OnAddRoomToCartClicked(Habitacion habitacion)
    {
        try
        {
            if (!_sessionService.IsLoggedIn)
            {
                bool goToLogin = await DisplayAlert(
                    "Login Required",
                    "You need to login to add items to cart. Would you like to login now?",
                    "Yes",
                    "No"
                );

                if (goToLogin)
                {
                    await Shell.Current.GoToAsync("//Login");
                }
                return;
            }

            if (_checkInDate >= _checkOutDate)
            {
                await DisplayAlert("Invalid Dates", "Check-out date must be after check-in date", "OK");
                return;
            }

            if (_guestCount > habitacion.CapacidadPersonas)
            {
                await DisplayAlert(
                    "Capacity Exceeded",
                    $"This room can accommodate up to {habitacion.CapacidadPersonas} guests. You selected {_guestCount} guests.",
                    "OK"
                );
                return;
            }

            var success = await _cartService.AddHabitacionAsync(habitacion, _checkInDate, _checkOutDate, _guestCount);

            if (success)
            {
                bool goToCart = await DisplayAlert(
                    "Added to Cart!",
                    $"{habitacion.Nombre} has been added to your cart.\n{_numberOfNights} nights: ${habitacion.PrecioPorNoche * _numberOfNights:F2}",
                    "View Cart",
                    "Continue Shopping"
                );

                if (goToCart)
                {
                    await Shell.Current.GoToAsync("//Cart");
                }
            }
            else
            {
                var errorMsg = !string.IsNullOrEmpty(_cartService.LastError) 
                    ? _cartService.LastError 
                    : "Could not add room to cart. Please try again.";
                await DisplayAlert("Error", errorMsg, "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"BookingPage OnAddRoomToCartClicked Error: {ex.Message}");
            await DisplayAlert("Error", $"Could not add room to cart: {ex.Message}", "OK");
        }
    }

    private void OnCheckInDateSelected(object sender, DateChangedEventArgs e)
    {
        _checkInDate = e.NewDate;

       
        if (_checkOutDate <= _checkInDate)
        {
            _checkOutDate = _checkInDate.AddDays(1);
            CheckOutDatePicker.Date = _checkOutDate;
        }

        CheckOutDatePicker.MinimumDate = _checkInDate.AddDays(1);
        UpdateNightsDisplay();
    }

    private void OnCheckOutDateSelected(object sender, DateChangedEventArgs e)
    {
        _checkOutDate = e.NewDate;
        UpdateNightsDisplay();
    }

    private void UpdateNightsDisplay()
    {
        _numberOfNights = (_checkOutDate - _checkInDate).Days;
        if (_numberOfNights > 0)
        {
            NightsDisplay.IsVisible = true;
            NightsLabel.Text = $"{_numberOfNights} night{(_numberOfNights > 1 ? "s" : "")}";
            
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (RoomsContainer.Children.Count > 0)
                {
                    await LoadRoomsAsync();
                }
            });
        }
        else
        {
            NightsDisplay.IsVisible = false;
        }
    }

    private void OnDecreaseGuestsClicked(object sender, EventArgs e)
    {
        if (_guestCount > 1)
        {
            _guestCount--;
            GuestsLabel.Text = _guestCount.ToString();
        }
    }

    private void OnIncreaseGuestsClicked(object sender, EventArgs e)
    {
        if (_guestCount < 10)
        {
            _guestCount++;
            GuestsLabel.Text = _guestCount.ToString();
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainMenu");
    }
}
