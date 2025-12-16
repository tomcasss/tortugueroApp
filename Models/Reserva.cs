using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace tortugueroApp.Models;

[Table("reserva")]
public class Reserva : BaseModel
{
    [PrimaryKey("id_reserva", shouldInsert: false)]
    public long? IdReserva { get; set; }

    [Column("id_usuario")]
    public long IdUsuario { get; set; }

    [Column("fecha_checkin")]
    public DateTime? FechaCheckin { get; set; }

    [Column("fecha_checkout")]
    public DateTime? FechaCheckout { get; set; }

    [Column("cantidad_personas")]
    public int? CantidadPersonas { get; set; }

    [Column("subtotal_habitacion")]
    public double SubtotalHabitacion { get; set; }

    [Column("subtotal_actividades")]
    public double SubtotalActividades { get; set; }

    [Column("total_pagar")]
    public double TotalPagar { get; set; }

    [Column("estado")]
    public string Estado { get; set; } = "carrito";

    [Column("fecha_reserva")]
    public DateTime FechaReserva { get; set; }

    [Column("metodo_pago")]
    public string? MetodoPago { get; set; }
}
