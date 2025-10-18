using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroMarketApi.Models;

[Table("mensajes")]
public class Mensaje
{
    [Key, Column("id")] public int Id { get; set; }

    [Required, Column("chat_id")] public int ChatId { get; set; }
    public Chat? Chat { get; set; }

    [Required, Column("remitente_id")] public int RemitenteId { get; set; } // FK a usuarios
    public Usuario? Remitente { get; set; }

    [Required, Column("mensaje")] public string Contenido { get; set; } = string.Empty;

    [Column("fecha_envio")] public DateTime? FechaEnvio { get; set; }
    [Column("leido")] public bool Leido { get; set; } = false;
}
