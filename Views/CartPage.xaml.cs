using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using tortugueroApp.Services;

namespace tortugueroApp.Views
{
    public partial class CartPage : ContentPage
    {
        private CartService _cartService;

        public CartPage()
        {
            InitializeComponent();
            _cartService = CartService.Instance;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
         
            _cartService.OnCartChanged += OnCartUpdated;
            
            await LoadCartAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
           
            _cartService.OnCartChanged -= OnCartUpdated;
        }

        private void OnCartUpdated()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await LoadCartAsync();
            });
        }

        private async Task LoadCartAsync()
        {
            try
            {
                LoadingIndicator.IsRunning = true;

                await _cartService.LoadOrCreateCartAsync();

                var totalItems = _cartService.TotalItems;

                if (totalItems == 0)
                {
                
                    EmptyCartView.IsVisible = true;
                    CartContentView.IsVisible = false;
                    CheckoutButtonContainer.IsVisible = false;
                }
                else
                {
                    
                    EmptyCartView.IsVisible = false;
                    CartContentView.IsVisible = true;
                    CheckoutButtonContainer.IsVisible = true;

                    await LoadRoomsAsync();
                    await LoadActivitiesAsync();
                    UpdateSummary();
                }

                LoadingIndicator.IsRunning = false;
            }
            catch (Exception ex)
            {
                LoadingIndicator.IsRunning = false;
                await DisplayAlert("Error", $"Could not load cart: {ex.Message}", "OK");
            }
        }

        private async Task LoadRoomsAsync()
        {
            RoomsContainer.Children.Clear();

            var roomItems = _cartService.GetHabitacionesItems();

            if (roomItems.Count == 0)
            {
                RoomsSection.IsVisible = false;
                return;
            }

            RoomsSection.IsVisible = true;

            foreach (var item in roomItems)
            {
                var itemCard = new Frame
                {
                    BackgroundColor = Color.FromArgb("#F8F9FA"),
                    CornerRadius = 8,
                    Padding = 12,
                    HasShadow = false,
                    BorderColor = Color.FromArgb("#E0E0E0"),
                    Margin = new Thickness(0, 5)
                };

                var grid = new Grid
                {
                    RowDefinitions = new RowDefinitionCollection
                    {
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto }
                    },
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    }
                };

                var nameLabel = new Label
                {
                    Text = item.Habitacion.Nombre,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#2E4E49")
                };
                Grid.SetRow(nameLabel, 0);
                Grid.SetColumn(nameLabel, 0);
                grid.Children.Add(nameLabel);

                var datesLabel = new Label
                {
                    Text = $"📅 {item.ReservaHabitacion.NumeroNoches} noche{(item.ReservaHabitacion.NumeroNoches > 1 ? "s" : "")}",
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                Grid.SetRow(datesLabel, 1);
                Grid.SetColumn(datesLabel, 0);
                grid.Children.Add(datesLabel);

                var priceLabel = new Label
                {
                    Text = $"${item.ReservaHabitacion.Subtotal:F2}",
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#11d4b4"),
                    VerticalOptions = LayoutOptions.Center
                };
                Grid.SetRow(priceLabel, 0);
                Grid.SetRowSpan(priceLabel, 2);
                Grid.SetColumn(priceLabel, 1);
                grid.Children.Add(priceLabel);

                var removeButton = new Button
                {
                    Text = "🗑️ Eliminar",
                    BackgroundColor = Color.FromArgb("#FF5252"),
                    TextColor = Colors.White,
                    FontSize = 12,
                    CornerRadius = 6,
                    Padding = new Thickness(10, 5),
                    HorizontalOptions = LayoutOptions.End
                };
                var itemIndex = roomItems.IndexOf(item);
                removeButton.Clicked += async (s, e) => await OnRemoveRoomClicked(itemIndex);
                Grid.SetRow(removeButton, 2);
                Grid.SetColumn(removeButton, 0);
                Grid.SetColumnSpan(removeButton, 2);
                grid.Children.Add(removeButton);

                itemCard.Content = grid;
                RoomsContainer.Children.Add(itemCard);
            }
        }

        private async Task LoadActivitiesAsync()
        {
            ActivitiesContainer.Children.Clear();

            var activityItems = _cartService.GetActividadesItems();

            if (activityItems.Count == 0)
            {
                ActivitiesSection.IsVisible = false;
                return;
            }

            ActivitiesSection.IsVisible = true;

            foreach (var item in activityItems)
            {
                var itemCard = new Frame
                {
                    BackgroundColor = Color.FromArgb("#F8F9FA"),
                    CornerRadius = 8,
                    Padding = 12,
                    HasShadow = false,
                    BorderColor = Color.FromArgb("#E0E0E0"),
                    Margin = new Thickness(0, 5)
                };

                var grid = new Grid
                {
                    RowDefinitions = new RowDefinitionCollection
                    {
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto }
                    },
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    }
                };

                var nameLabel = new Label
                {
                    Text = item.Actividad.Nombre,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#2E4E49")
                };
                Grid.SetRow(nameLabel, 0);
                Grid.SetColumn(nameLabel, 0);
                grid.Children.Add(nameLabel);

                var detailsLabel = new Label
                {
                    Text = $"📅 {item.ReservaActividad.FechaActividad:dd/MMM/yyyy} • 👥 {item.ReservaActividad.CantidadPersonas} persona{(item.ReservaActividad.CantidadPersonas > 1 ? "s" : "")}",
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                Grid.SetRow(detailsLabel, 1);
                Grid.SetColumn(detailsLabel, 0);
                grid.Children.Add(detailsLabel);

                var priceLabel = new Label
                {
                    Text = $"${item.ReservaActividad.Subtotal:F2}",
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#11d4b4"),
                    VerticalOptions = LayoutOptions.Center
                };
                Grid.SetRow(priceLabel, 0);
                Grid.SetRowSpan(priceLabel, 2);
                Grid.SetColumn(priceLabel, 1);
                grid.Children.Add(priceLabel);

                var removeButton = new Button
                {
                    Text = "🗑️ Eliminar",
                    BackgroundColor = Color.FromArgb("#FF5252"),
                    TextColor = Colors.White,
                    FontSize = 12,
                    CornerRadius = 6,
                    Padding = new Thickness(10, 5),
                    HorizontalOptions = LayoutOptions.End
                };
                var activityIndex = activityItems.IndexOf(item);
                removeButton.Clicked += async (s, e) => await OnRemoveActivityClicked(activityIndex);
                Grid.SetRow(removeButton, 2);
                Grid.SetColumn(removeButton, 0);
                Grid.SetColumnSpan(removeButton, 2);
                grid.Children.Add(removeButton);

                itemCard.Content = grid;
                ActivitiesContainer.Children.Add(itemCard);
            }
        }

        private void UpdateSummary()
        {
            SubtotalRoomsLabel.Text = $"${_cartService.SubtotalHabitacion:F2}";
            SubtotalActivitiesLabel.Text = $"${_cartService.SubtotalActividades:F2}";
            TotalLabel.Text = $"${_cartService.TotalAmount:F2}";
        }

        private async Task OnRemoveRoomClicked(int index)
        {
            bool confirm = await DisplayAlert(
                "Confirm",
                "Do you want to remove this room from the cart?",
                "Yes",
                "No"
            );

            if (confirm)
            {
                try
                {
                    await _cartService.RemoveHabitacionAsync(index);
                    await DisplayAlert("Success", "Room removed from cart", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"No se pudo eliminar: {ex.Message}", "OK");
                }
            }
        }

        private async Task OnRemoveActivityClicked(int index)
        {
            bool confirm = await DisplayAlert(
                "Confirm",
                "Do you want to remove this activity from the cart?",
                "Yes",
                "No"
            );

            if (confirm)
            {
                try
                {
                    await _cartService.RemoveActividadAsync(index);
                    await DisplayAlert("Success", "Activity removed from cart", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"No se pudo eliminar: {ex.Message}", "OK");
                }
            }
        }

        private async void OnCheckoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Confirm Payment",
                $"Total to pay: ${_cartService.TotalAmount:F2}\n\nDo you want to proceed with payment?",
                "Yes, Pay",
                "Cancel"
            );

            if (confirm)
            {
                try
                {
                    LoadingIndicator.IsRunning = true;
                    CheckoutButton.IsEnabled = false;

                    
                    await Task.Delay(2000);

                    bool success = await _cartService.CheckoutAsync("Tarjeta de Crédito");

                    LoadingIndicator.IsRunning = false;

                    if (success)
                    {
                        await DisplayAlert(
                            "Payment Successful!",
                            "Your booking has been confirmed. You can view it in 'My Bookings'.",
                            "OK"
                        );

                        await Shell.Current.GoToAsync("//MyBookings");
                    }
                    else
                    {
                        CheckoutButton.IsEnabled = true;
                        await DisplayAlert(
                            "Payment Failed",
                            $"Could not process payment. Error: {_cartService.LastError ?? "Unknown error"}",
                            "OK"
                        );
                    }
                }
                catch (Exception ex)
                {
                    LoadingIndicator.IsRunning = false;
                    CheckoutButton.IsEnabled = true;
                    await DisplayAlert("Error", $"Could not complete payment: {ex.Message}", "OK");
                }
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MainMenu");
        }
    }
}

