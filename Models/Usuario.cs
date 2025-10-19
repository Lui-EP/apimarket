using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AgroMarketApi.Models;

[Table("usuarios")]
public class Usuario
{
    [Key, Column("id")] public int Id { get; set; }

    [Required, Column("tipo")] public string Tipo { get; set; } = string.Empty; // productor/empresa
    [Required, Column("nombre")] public string Nombre { get; set; } = string.Empty;
    [Required, Column("email")] public string Email { get; set; } = string.Empty;
    [Required, Column("password")] public string Password { get; set; } = string.Empty; // plano para pruebas

    [Column("ubicacion")] public string? Ubicacion { get; set; }
    [Column("telefono")] public string? Telefono { get; set; }
    [Column("activo")] public bool Activo { get; set; } = true;
    [Column("fecha_registro")] public DateTime? FechaRegistro { get; set; }
    [Column("ultima_conexion")] public DateTime? UltimaConexion { get; set; }
    
    [JsonIgnore]     // 👈 evita Usuario.Productos -> Producto.Productor -> Usuario...
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
