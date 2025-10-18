using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroMarketApi.Models;

[Table("intereses")]
public class Interes
{
    [Key, Column("id")] public int Id { get; set; }

    [Required, Column("producto_id")] public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    [Required, Column("empresa_id")] public int EmpresaId { get; set; } // FK a usuarios
    public Usuario? Empresa { get; set; }

    [Column("notas")] public string? Notas { get; set; }
    [Column("fecha_interes")] public DateTime? FechaInteres { get; set; }
    [Column("activo")] public bool Activo { get; set; } = true;
}
