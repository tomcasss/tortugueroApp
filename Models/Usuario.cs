using Postgrest.Attributes;
using Postgrest.Models;

namespace tortugueroApp.Models;

[Table("usuario")]
public class Usuario : BaseModel
{
    [PrimaryKey("id_usuario", false)]
    public int IdUsuario { get; set; }

    [Column("nombre_completo")]
    public string? NombreCompleto { get; set; }

    [Column("edad")]
    public int? Edad { get; set; }

    [Column("domicilio")]
    public string? Domicilio { get; set; }

    [Column("correo")]
    public string? Correo { get; set; }

    [Column("nombre_usuario")]
    public string? NombreUsuario { get; set; }

    [Column("contrasenia_hash")]
    public string? ContraseniaHash { get; set; }

    [Column("numero_telefono")]
    public string? NumeroTelefono { get; set; }

    [Column("fecha_creacion")]
    public DateTime? FechaCreacion { get; set; }
}
