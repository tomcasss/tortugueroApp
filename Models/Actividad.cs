using Postgrest.Attributes;
using Postgrest.Models;

namespace tortugueroApp.Models;

[Table("actividad")]
public class Actividad : BaseModel
{
    [PrimaryKey("id_actividad", false)]
    [Column("id_actividad")]
    public long IdActividad { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("precio")]
    public double Precio { get; set; }

    [Column("duracion_horas")]
    public int? DuracionHoras { get; set; }

    [Column("capacidad_maxima")]
    public int? CapacidadMaxima { get; set; }

    [Column("categoria")]
    public string? Categoria { get; set; }

    [Column("imagen_url")]
    public string? ImagenUrl { get; set; }

    [Column("disponible")]
    public bool Disponible { get; set; }
}
