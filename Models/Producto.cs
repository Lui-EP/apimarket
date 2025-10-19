using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AgroMarketApi.Models;

[Table("productos")]
public class Producto
{
    [Key, Column("id")] 
    public int Id { get; set; }

    [Required, Column("productor_id")] 
    public int ProductorId { get; set; }
    public Usuario? Productor { get; set; }

    [Required, Column("nombre")] 
    public string Nombre { get; set; } = string.Empty;

    [Required, Column("precio", TypeName = "numeric(12,2)")] 
    public decimal Precio { get; set; }

    [Required, Column("volumen", TypeName = "numeric(12,2)")] 
    public decimal Volumen { get; set; } // toneladas

    [Column("ubicacion")] 
    public string? Ubicacion { get; set; }

    [Column("descripcion")] 
    public string? Descripcion { get; set; }

    [Column("fecha_publicacion")] 
    public DateTime? FechaPublicacion { get; set; }

    [Column("activo")] 
    public bool Activo { get; set; } = true;

    [JsonIgnore] // 👈 Esto evita el ciclo en GET /api/Intereses
    public ICollection<Interes> Intereses { get; set; } = new List<Interes>();
}
