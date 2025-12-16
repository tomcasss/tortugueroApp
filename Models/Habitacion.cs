using Postgrest.Attributes;
using Postgrest.Models;
using System.Text.Json.Serialization;

namespace tortugueroApp.Models;

[Table("habitacion")]
public class Habitacion : BaseModel
{
    [PrimaryKey("id_habitacion", false)]
    [Column("id_habitacion")]
    public long IdHabitacion { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("precio_por_noche")]
    public double PrecioPorNoche { get; set; }

    [Column("capacidad_personas")]
    public int CapacidadPersonas { get; set; }

    [Column("disponible")]
    public bool Disponible { get; set; }

    [Column("imagen_url")]
    public string? ImagenUrl { get; set; }

    [Column("amenidades")]
    [JsonPropertyName("amenidades")]
    public List<string>? Amenidades { get; set; }
}
