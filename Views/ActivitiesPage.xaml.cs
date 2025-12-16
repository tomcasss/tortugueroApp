using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tortugueroApp.Models;
using tortugueroApp.Services;

namespace tortugueroApp.Views
{
    public partial class ActivitiesPage : ContentPage
    {
        private List<Actividad> _actividades;

        public ActivitiesPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadActivitiesAsync();
        }

        private async Task LoadActivitiesAsync()
        {
            try
            {
                LoadingIndicator.IsRunning = true;
                ActivitiesContainer.IsVisible = false;

                var supabase = await SupabaseService.GetClientAsync();
                var response = await supabase
                    .From<Actividad>()
                    .Where(a => a.Disponible == true)
                    .Order("nombre", Postgrest.Constants.Ordering.Ascending)
                    .Get();

                _actividades = response.Models;

              
                ActivitiesContainer.Children.Clear();

                foreach (var actividad in _actividades)
                {
                    var activityCard = CreateActivityCard(actividad);
                    ActivitiesContainer.Children.Add(activityCard);
                }

                LoadingIndicator.IsRunning = false;
                ActivitiesContainer.IsVisible = true;
            }
            catch (Exception ex)
            {
                LoadingIndicator.IsRunning = false;
                await DisplayAlert("Error", $"No se pudieron cargar las actividades: {ex.Message}", "OK");
            }
        }

        private Frame CreateActivityCard(Actividad actividad)
        {
            var card = new Frame
            {
                BackgroundColor = Colors.White,
                CornerRadius = 12,
                Padding = 0,
                HasShadow = true,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var grid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = 200 },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };

            
            var image = new Image
            {
                Source = string.IsNullOrEmpty(actividad.ImagenUrl) 
                    ? "dotnet_bot.png" 
                    : actividad.ImagenUrl,
                Aspect = Aspect.AspectFill,
                HeightRequest = 200
            };
            Grid.SetRow(image, 0);
            grid.Children.Add(image);

          
            var categoryFrame = new Frame
            {
                BackgroundColor = Color.FromArgb("#11d4b4"),
                CornerRadius = 15,
                Padding = new Thickness(12, 6),
                HasShadow = false,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(10)
            };
            var categoryLabel = new Label
            {
                Text = actividad.Categoria,
                TextColor = Colors.White,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold
            };
            categoryFrame.Content = categoryLabel;
            Grid.SetRow(categoryFrame, 0);
            grid.Children.Add(categoryFrame);

           
            var contentStack = new VerticalStackLayout
            {
                Padding = 15,
                Spacing = 8
            };

            var nameLabel = new Label
            {
                Text = actividad.Nombre,
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#2E4E49")
            };
            contentStack.Children.Add(nameLabel);

            var descLabel = new Label
            {
                Text = actividad.Descripcion,
                FontSize = 14,
                TextColor = Colors.Gray,
                MaxLines = 3,
                LineBreakMode = LineBreakMode.TailTruncation
            };
            contentStack.Children.Add(descLabel);

          
            var infoGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                Margin = new Thickness(0, 8, 0, 0)
            };

            var durationStack = new HorizontalStackLayout { Spacing = 5 };
            durationStack.Children.Add(new Label
            {
                Text = "⏱️",
                FontSize = 16
            });
            durationStack.Children.Add(new Label
            {
                Text = $"{actividad.DuracionHoras}h",
                FontSize = 14,
                TextColor = Colors.Gray,
                VerticalOptions = LayoutOptions.Center
            });
            Grid.SetColumn(durationStack, 0);
            infoGrid.Children.Add(durationStack);

            var capacityStack = new HorizontalStackLayout { Spacing = 5 };
            capacityStack.Children.Add(new Label
            {
                Text = "👥",
                FontSize = 16
            });
            capacityStack.Children.Add(new Label
            {
                Text = $"Max {actividad.CapacidadMaxima}",
                FontSize = 14,
                TextColor = Colors.Gray,
                VerticalOptions = LayoutOptions.Center
            });
            Grid.SetColumn(capacityStack, 1);
            infoGrid.Children.Add(capacityStack);

            contentStack.Children.Add(infoGrid);

          
            var bottomGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                Margin = new Thickness(0, 10, 0, 0)
            };

            var priceLabel = new Label
            {
                Text = $"${actividad.Precio:F2}",
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#11d4b4"),
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(priceLabel, 0);
            bottomGrid.Children.Add(priceLabel);

            var addButton = new Button
            {
                Text = "Add to Cart",
                BackgroundColor = Color.FromArgb("#11d4b4"),
                TextColor = Colors.White,
                CornerRadius = 8,
                HorizontalOptions = LayoutOptions.End,
                Padding = new Thickness(20, 10)
            };
            addButton.Clicked += async (s, e) => await OnAddToCartClicked(actividad);
            Grid.SetColumn(addButton, 1);
            bottomGrid.Children.Add(addButton);

            contentStack.Children.Add(bottomGrid);

            Grid.SetRow(contentStack, 1);
            grid.Children.Add(contentStack);

            card.Content = grid;
            return card;
        }

        private async Task OnAddToCartClicked(Actividad actividad)
        {
            try
            {
               
                var datePicker = new DatePicker
                {
                    MinimumDate = DateTime.Today,
                    MaximumDate = DateTime.Today.AddMonths(6),
                    Date = DateTime.Today.AddDays(1)
                };

                var peopleStepper = new Stepper
                {
                    Minimum = 1,
                    Maximum = (double)(actividad.CapacidadMaxima ?? 10),
                    Value = 2,
                    Increment = 1
                };

                var peopleLabel = new Label
                {
                    Text = "2 personas",
                    HorizontalOptions = LayoutOptions.Center
                };

                peopleStepper.ValueChanged += (s, e) =>
                {
                    peopleLabel.Text = $"{(int)e.NewValue} persona{((int)e.NewValue > 1 ? "s" : "")}";
                };

                var contentStack = new VerticalStackLayout
                {
                    Spacing = 15,
                    Children =
                    {
                        new Label { Text = "Fecha de la actividad:", FontAttributes = FontAttributes.Bold },
                        datePicker,
                        new Label { Text = "Cantidad de personas:", FontAttributes = FontAttributes.Bold },
                        peopleStepper,
                        peopleLabel
                    }
                };

                var popup = new ContentPage
                {
                    Content = new Frame
                    {
                        Content = new VerticalStackLayout
                        {
                            Spacing = 20,
                            Padding = 20,
                            Children =
                            {
                                new Label
                                {
                                    Text = actividad.Nombre,
                                    FontSize = 20,
                                    FontAttributes = FontAttributes.Bold
                                },
                                contentStack,
                                new Button
                                {
                                    Text = "Agregar al Carrito",
                                    BackgroundColor = Color.FromArgb("#11d4b4"),
                                    TextColor = Colors.White,
                                    Command = new Command(async () =>
                                    {
                                        await AddToCart(actividad, datePicker.Date, (int)peopleStepper.Value);
                                        await Navigation.PopModalAsync();
                                    })
                                },
                                new Button
                                {
                                    Text = "Cancelar",
                                    BackgroundColor = Colors.Gray,
                                    TextColor = Colors.White,
                                    Command = new Command(async () => await Navigation.PopModalAsync())
                                }
                            }
                        }
                    }
                };

                await Navigation.PushModalAsync(popup);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al abrir formulario: {ex.Message}", "OK");
            }
        }

        private async Task AddToCart(Actividad actividad, DateTime fecha, int cantidadPersonas)
        {
            try
            {
                
                var sessionService = SessionService.Instance;
                if (!sessionService.IsLoggedIn)
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

                
                if (actividad.CapacidadMaxima.HasValue && cantidadPersonas > actividad.CapacidadMaxima.Value)
                {
                    await DisplayAlert(
                        "Capacity Exceeded",
                        $"This activity can accommodate up to {actividad.CapacidadMaxima.Value} people. You selected {cantidadPersonas} people.",
                        "OK"
                    );
                    return;
                }

                var success = await CartService.Instance.AddActividadAsync(actividad, fecha, cantidadPersonas);
                
                if (success)
                {
                    await DisplayAlert("Success", $"{actividad.Nombre} has been added to your cart", "OK");
                }
                else
                {
                    var cartService = CartService.Instance;
                    var errorMsg = !string.IsNullOrEmpty(cartService.LastError) 
                        ? cartService.LastError 
                        : "Could not add activity to cart. Please try again.";
                    await DisplayAlert("Error", errorMsg, "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ActivitiesPage AddToCart Error: {ex.Message}");
                await DisplayAlert("Error", $"Could not add to cart: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MainMenu");
        }
    }
}
