using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroMarketApi.Models;

[Table("chats")]
public class Chat
{
    [Key, Column("id")] public int Id { get; set; }

    [Required, Column("producto_id")] public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    [Required, Column("productor_id")] public int ProductorId { get; set; }
    public Usuario? Productor { get; set; }

    [Required, Column("empresa_id")] public int EmpresaId { get; set; }
    public Usuario? Empresa { get; set; }

    [Column("fecha_creacion")] public DateTime? FechaCreacion { get; set; }

    public ICollection<Mensaje> Mensajes { get; set; } = new List<Mensaje>();
}
