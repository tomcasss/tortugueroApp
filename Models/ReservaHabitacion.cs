using Postgrest.Attributes;
using Postgrest.Models;

namespace tortugueroApp.Models;

[Table("reservahabitacion")]
public class ReservaHabitacion : BaseModel
{
    [PrimaryKey("id_reserva_habitacion", shouldInsert: false)]
    public long? IdReservaHabitacion { get; set; }

    [Column("id_reserva")]
    public long IdReserva { get; set; }

    [Column("id_habitacion")]
    public long IdHabitacion { get; set; }

    [Column("precio_por_noche")]
    public double PrecioPorNoche { get; set; }

    [Column("numero_noches")]
    public int NumeroNoches { get; set; }

    [Column("subtotal")]
    public double Subtotal { get; set; }
}
