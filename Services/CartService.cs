using tortugueroApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace tortugueroApp.Services;

public class CartService
{
    private static CartService? _instance;
    public static CartService Instance => _instance ??= new CartService();

    private readonly SessionService _sessionService;

    private List<CartItemHabitacion> _habitacionesItems;
    private List<CartItemActividad> _actividadesItems;
    private DateTime? _cartCheckIn;
    private DateTime? _cartCheckOut;
    private int _cartGuestCount;

    public event Action? OnCartChanged;

    public int TotalItems => _habitacionesItems.Count + _actividadesItems.Count;
    public double TotalAmount => _habitacionesItems.Sum(x => x.ReservaHabitacion.Subtotal) + _actividadesItems.Sum(x => x.ReservaActividad.Subtotal);
    public double SubtotalHabitacion => _habitacionesItems.Sum(x => x.ReservaHabitacion.Subtotal);
    public double SubtotalActividades => _actividadesItems.Sum(x => x.ReservaActividad.Subtotal);

    private CartService()
    {
        _sessionService = SessionService.Instance;
        _habitacionesItems = new List<CartItemHabitacion>();
        _actividadesItems = new List<CartItemActividad>();
    }

    public Task<bool> LoadOrCreateCartAsync()
    {
        return Task.FromResult(true);
    }

    public string? LastError { get; private set; }

    public Task<bool> AddHabitacionAsync(Habitacion habitacion, DateTime checkin, DateTime checkout, int personas)
    {
        try
        {
            if (!_sessionService.IsLoggedIn || _sessionService.CurrentUser == null)
                return Task.FromResult(false);

            var noches = (checkout - checkin).Days;
            if (noches <= 0) return Task.FromResult(false);

            var subtotal = habitacion.PrecioPorNoche * noches;

            _cartCheckIn = checkin;
            _cartCheckOut = checkout;
            _cartGuestCount = personas;

            var item = new ReservaHabitacion
            {
                IdHabitacion = habitacion.IdHabitacion,
                PrecioPorNoche = habitacion.PrecioPorNoche,
                NumeroNoches = noches,
                Subtotal = subtotal
            };

            _habitacionesItems.Add(new CartItemHabitacion
            {
                ReservaHabitacion = item,
                Habitacion = habitacion
            });

            OnCartChanged?.Invoke();
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            LastError = $"AddHabitacionAsync: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"AddHabitacionAsync Error: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    public Task<bool> AddActividadAsync(Actividad actividad, DateTime fecha, int personas)
    {
        try
        {
            if (!_sessionService.IsLoggedIn || _sessionService.CurrentUser == null)
                return Task.FromResult(false);

            var subtotal = actividad.Precio * personas;

            var item = new ReservaActividad
            {
                IdActividad = actividad.IdActividad,
                FechaActividad = fecha,
                CantidadPersonas = personas,
                PrecioUnitario = actividad.Precio,
                Subtotal = subtotal
            };

            _actividadesItems.Add(new CartItemActividad
            {
                ReservaActividad = item,
                Actividad = actividad
            });

            OnCartChanged?.Invoke();
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            LastError = $"AddActividadAsync: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"AddActividadAsync Error: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    public Task<bool> RemoveHabitacionAsync(int index)
    {
        try
        {
            if (index >= 0 && index < _habitacionesItems.Count)
            {
                _habitacionesItems.RemoveAt(index);
                OnCartChanged?.Invoke();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> RemoveActividadAsync(int index)
    {
        try
        {
            if (index >= 0 && index < _actividadesItems.Count)
            {
                _actividadesItems.RemoveAt(index);
                OnCartChanged?.Invoke();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<bool> CheckoutAsync(string metodoPago)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== CheckoutAsync START ===");
            
            if (!_sessionService.IsLoggedIn || _sessionService.CurrentUser == null)
            {
                LastError = "User not logged in";
                System.Diagnostics.Debug.WriteLine("CheckoutAsync: User not logged in");
                return false;
            }

            if (TotalItems == 0)
            {
                LastError = "Cart is empty";
                System.Diagnostics.Debug.WriteLine("CheckoutAsync: Cart is empty");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"CheckoutAsync: TotalItems={TotalItems}, User={_sessionService.CurrentUser.IdUsuario}");
            System.Diagnostics.Debug.WriteLine($"CheckoutAsync: Habitaciones={_habitacionesItems.Count}, Actividades={_actividadesItems.Count}");
            System.Diagnostics.Debug.WriteLine($"CheckoutAsync: CheckIn={_cartCheckIn}, CheckOut={_cartCheckOut}, Guests={_cartGuestCount}");

            var client = await SupabaseService.GetClientAsync();
            System.Diagnostics.Debug.WriteLine("CheckoutAsync: Got Supabase client");

            var newReserva = new Reserva
            {
                IdUsuario = _sessionService.CurrentUser.IdUsuario,
                Estado = "pagada",
                FechaReserva = DateTime.Now,
                FechaCheckin = _cartCheckIn,
                FechaCheckout = _cartCheckOut,
                CantidadPersonas = _cartGuestCount,
                SubtotalHabitacion = SubtotalHabitacion,
                SubtotalActividades = SubtotalActividades,
                TotalPagar = TotalAmount,
                MetodoPago = metodoPago
            };

            System.Diagnostics.Debug.WriteLine($"CheckoutAsync: Inserting Reserva - Total: {TotalAmount}");
            var createdReserva = await client.From<Reserva>().Insert(newReserva);
            
            if (createdReserva?.Models == null || createdReserva.Models.Count == 0)
            {
                LastError = "Failed to create Reserva - no response from database";
                System.Diagnostics.Debug.WriteLine("CheckoutAsync: Failed to create Reserva");
                return false;
            }
            
            var reservaId = createdReserva.Models[0].IdReserva!.Value;
            System.Diagnostics.Debug.WriteLine($"CheckoutAsync: Created Reserva with ID={reservaId}");

            foreach (var item in _habitacionesItems)
            {
                item.ReservaHabitacion.IdReserva = reservaId;
                System.Diagnostics.Debug.WriteLine($"CheckoutAsync: Inserting ReservaHabitacion - Room={item.Habitacion.Nombre}");
                await client.From<ReservaHabitacion>().Insert(item.ReservaHabitacion);
            }

            foreach (var item in _actividadesItems)
            {
                item.ReservaActividad.IdReserva = reservaId;
                System.Diagnostics.Debug.WriteLine($"CheckoutAsync: Inserting ReservaActividad - Activity={item.Actividad.Nombre}");
                await client.From<ReservaActividad>().Insert(item.ReservaActividad);
            }

            System.Diagnostics.Debug.WriteLine("CheckoutAsync: All inserts successful, clearing cart");

            _habitacionesItems.Clear();
            _actividadesItems.Clear();
            _cartCheckIn = null;
            _cartCheckOut = null;
            _cartGuestCount = 0;
            OnCartChanged?.Invoke();

            System.Diagnostics.Debug.WriteLine("=== CheckoutAsync SUCCESS ===");
            return true;
        }
        catch (Exception ex)
        {
            LastError = $"CheckoutAsync: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"CheckoutAsync Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"CheckoutAsync StackTrace: {ex.StackTrace}");
            return false;
        }
    }

    public Task ClearCartAsync()
    {
        _habitacionesItems.Clear();
        _actividadesItems.Clear();
        _cartCheckIn = null;
        _cartCheckOut = null;
        _cartGuestCount = 0;
        OnCartChanged?.Invoke();
        return Task.CompletedTask;
    }

    public List<CartItemHabitacion> GetHabitacionesItems() => new List<CartItemHabitacion>(_habitacionesItems);
    public List<CartItemActividad> GetActividadesItems() => new List<CartItemActividad>(_actividadesItems);
}

public class CartItemHabitacion
{
    public ReservaHabitacion ReservaHabitacion { get; set; } = null!;
    public Habitacion Habitacion { get; set; } = null!;
}

public class CartItemActividad
{
    public ReservaActividad ReservaActividad { get; set; } = null!;
    public Actividad Actividad { get; set; } = null!;
}
