using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace tortugueroApp.Models;

[Table("reservaactividad")]
public class ReservaActividad : BaseModel
{
    [PrimaryKey("id_reserva_actividad", shouldInsert: false)]
    public long? IdReservaActividad { get; set; }

    [Column("id_reserva")]
    public long IdReserva { get; set; }

    [Column("id_actividad")]
    public long IdActividad { get; set; }

    [Column("fecha_actividad")]
    public DateTime FechaActividad { get; set; }

    [Column("cantidad_personas")]
    public int CantidadPersonas { get; set; }

    [Column("precio_unitario")]
    public double PrecioUnitario { get; set; }

    [Column("subtotal")]
    public double Subtotal { get; set; }
}
