using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tortugueroApp.Models;
using tortugueroApp.Services;

namespace tortugueroApp.Views
{
    public partial class MyBookingsPage : ContentPage
    {
        public MyBookingsPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadBookingsAsync();
        }

        private async Task LoadBookingsAsync()
        {
            try
            {
                LoadingIndicator.IsRunning = true;
                BookingsContainer.Children.Clear();

                var userId = SessionService.Instance.CurrentUserId;
                if (userId == 0)
                {
                    await DisplayAlert("Error", "You must login to view your bookings", "OK");
                    await Shell.Current.GoToAsync("//Login");
                    return;
                }

                var supabase = await SupabaseService.GetClientAsync();
                
                
                var response = await supabase
                    .From<Reserva>()
                    .Where(r => r.IdUsuario == userId)
                    .Where(r => r.Estado == "pagada" || r.Estado == "confirmada")
                    .Order("fecha_reserva", Postgrest.Constants.Ordering.Descending)
                    .Get();

                var reservas = response.Models;

                if (reservas == null || reservas.Count == 0)
                {
                    EmptyBookingsView.IsVisible = true;
                    BookingsContainer.IsVisible = false;
                }
                else
                {
                    EmptyBookingsView.IsVisible = false;
                    BookingsContainer.IsVisible = true;

                    foreach (var reserva in reservas)
                    {
                        var bookingCard = await CreateBookingCard(reserva);
                        BookingsContainer.Children.Add(bookingCard);
                    }
                }

                LoadingIndicator.IsRunning = false;
            }
            catch (Exception ex)
            {
                LoadingIndicator.IsRunning = false;
                await DisplayAlert("Error", $"Could not load bookings: {ex.Message}", "OK");
            }
        }

        private async Task<Frame> CreateBookingCard(Reserva reserva)
        {
            var card = new Frame
            {
                BackgroundColor = Colors.White,
                CornerRadius = 12,
                Padding = 0,
                HasShadow = true,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var mainStack = new VerticalStackLayout
            {
                Spacing = 0
            };

          
            var headerGrid = new Grid
            {
                BackgroundColor = reserva.Estado == "confirmada" 
                    ? Color.FromArgb("#4CAF50") 
                    : Color.FromArgb("#11d4b4"),
                Padding = 15,
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var bookingIdLabel = new Label
            {
                Text = $"Booking #{reserva.IdReserva}",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            };
            Grid.SetColumn(bookingIdLabel, 0);
            headerGrid.Children.Add(bookingIdLabel);

            var statusLabel = new Label
            {
                Text = reserva.Estado == "confirmada" ? "✓ Confirmed" : "✓ Paid",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(statusLabel, 1);
            headerGrid.Children.Add(statusLabel);

            mainStack.Children.Add(headerGrid);

          
            var contentStack = new VerticalStackLayout
            {
                Padding = 15,
                Spacing = 12
            };

        
            var dateGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };
            dateGrid.Children.Add(new Label
            {
                Text = "📅",
                FontSize = 18,
                VerticalOptions = LayoutOptions.Center
            });
            var fechaLabel = new Label
            {
                Text = $"Booked on {reserva.FechaReserva:dd/MMM/yyyy}",
                FontSize = 14,
                TextColor = Colors.Gray,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetColumn(fechaLabel, 1);
            dateGrid.Children.Add(fechaLabel);
            contentStack.Children.Add(dateGrid);

          
            if (reserva.FechaCheckin.HasValue && reserva.FechaCheckout.HasValue)
            {
                var datesGrid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Star }
                    }
                };
                datesGrid.Children.Add(new Label
                {
                    Text = "🏨",
                    FontSize = 18,
                    VerticalOptions = LayoutOptions.Center
                });
                var stayLabel = new Label
                {
                    Text = $"Stay: {reserva.FechaCheckin.Value:dd/MMM} - {reserva.FechaCheckout.Value:dd/MMM/yyyy}",
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(10, 0, 0, 0)
                };
                Grid.SetColumn(stayLabel, 1);
                datesGrid.Children.Add(stayLabel);
                contentStack.Children.Add(datesGrid);
            }

       
            var supabase = await SupabaseService.GetClientAsync();

            
            var roomsResponse = await supabase
                .From<ReservaHabitacion>()
                .Where(rh => rh.IdReserva == reserva.IdReserva)
                .Get();

            if (roomsResponse.Models.Count > 0)
            {
                var roomsLabel = new Label
                {
                    Text = $"🏠 {roomsResponse.Models.Count} room{(roomsResponse.Models.Count > 1 ? "s" : "")}",
                    FontSize = 14,
                    TextColor = Color.FromArgb("#2E4E49"),
                    FontAttributes = FontAttributes.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                };
                contentStack.Children.Add(roomsLabel);

                foreach (var room in roomsResponse.Models)
                {
                    try
                    {
                        var habitacion = await supabase
                            .From<Habitacion>()
                            .Where(h => h.IdHabitacion == room.IdHabitacion)
                            .Single();

                        var roomDetail = new Label
                        {
                            Text = $"  • {habitacion.Nombre} - {room.NumeroNoches} night{(room.NumeroNoches > 1 ? "s" : "")} (${room.Subtotal:F2})",
                            FontSize = 13,
                            TextColor = Colors.Gray,
                            Margin = new Thickness(10, 0, 0, 2)
                        };
                        contentStack.Children.Add(roomDetail);
                    }
                    catch { }
                }
            }

            
            var activitiesResponse = await supabase
                .From<ReservaActividad>()
                .Where(ra => ra.IdReserva == reserva.IdReserva)
                .Get();

            if (activitiesResponse.Models.Count > 0)
            {
                var activitiesLabel = new Label
                {
                    Text = $"⛰️ {activitiesResponse.Models.Count} activit{(activitiesResponse.Models.Count > 1 ? "ies" : "y")}",
                    FontSize = 14,
                    TextColor = Color.FromArgb("#2E4E49"),
                    FontAttributes = FontAttributes.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                };
                contentStack.Children.Add(activitiesLabel);

                foreach (var activity in activitiesResponse.Models)
                {
                    try
                    {
                        var actividad = await supabase
                            .From<Actividad>()
                            .Where(a => a.IdActividad == activity.IdActividad)
                            .Single();

                        var activityDetail = new Label
                        {
                            Text = $"  • {actividad.Nombre} - {activity.FechaActividad:dd/MMM/yyyy} ({activity.CantidadPersonas} person{(activity.CantidadPersonas > 1 ? "s" : "")}) (${activity.Subtotal:F2})",
                            FontSize = 13,
                            TextColor = Colors.Gray,
                            Margin = new Thickness(10, 0, 0, 2)
                        };
                        contentStack.Children.Add(activityDetail);
                    }
                    catch { }
                }
            }

            
            var separator = new BoxView
            {
                HeightRequest = 1,
                Color = Color.FromArgb("#E0E0E0"),
                Margin = new Thickness(0, 10)
            };
            contentStack.Children.Add(separator);

            var totalGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var totalTextLabel = new Label
            {
                Text = "Total Paid:",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#2E4E49"),
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(totalTextLabel, 0);
            totalGrid.Children.Add(totalTextLabel);

            var totalAmountLabel = new Label
            {
                Text = $"${reserva.TotalPagar:F2}",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#11d4b4"),
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(totalAmountLabel, 1);
            totalGrid.Children.Add(totalAmountLabel);

            contentStack.Children.Add(totalGrid);

           
            if (!string.IsNullOrEmpty(reserva.MetodoPago))
            {
                var paymentLabel = new Label
                {
                    Text = $"💳 Paid with: {reserva.MetodoPago}",
                    FontSize = 12,
                    TextColor = Colors.Gray,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                contentStack.Children.Add(paymentLabel);
            }

            mainStack.Children.Add(contentStack);
            card.Content = mainStack;

            return card;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MainMenu");
        }
    }
}
