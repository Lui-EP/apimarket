using Microsoft.EntityFrameworkCore;
using AgroMarketApi.Models;

namespace AgroMarketApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Interes> Intereses => Set<Interes>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Mensaje> Mensajes => Set<Mensaje>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // usuarios
        b.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // productos
        b.Entity<Producto>()
            .HasOne(p => p.Productor)
            .WithMany(u => u.Productos)
            .HasForeignKey(p => p.ProductorId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<Producto>()
            .Property(p => p.Precio).HasPrecision(12, 2);
        b.Entity<Producto>()
            .Property(p => p.Volumen).HasPrecision(12, 2);

        // intereses (UNIQUE producto_id + empresa_id)
        b.Entity<Interes>()
            .HasIndex(i => new { i.ProductoId, i.EmpresaId })
            .IsUnique();
        b.Entity<Interes>()
            .HasOne(i => i.Producto)
            .WithMany(p => p.Intereses)
            .HasForeignKey(i => i.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<Interes>()
            .HasOne(i => i.Empresa)
            .WithMany() // si no quieres nav inversa
            .HasForeignKey(i => i.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        // chats (UNIQUE producto_id + empresa_id)
        b.Entity<Chat>()
            .HasIndex(c => new { c.ProductoId, c.EmpresaId })
            .IsUnique();
        b.Entity<Chat>()
            .HasOne(c => c.Producto)
            .WithMany()
            .HasForeignKey(c => c.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<Chat>()
            .HasOne(c => c.Productor)
            .WithMany()
            .HasForeignKey(c => c.ProductorId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<Chat>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        // mensajes
        b.Entity<Mensaje>()
            .HasIndex(m => m.ChatId);
        b.Entity<Mensaje>()
            .HasOne(m => m.Chat)
            .WithMany(c => c.Mensajes)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<Mensaje>()
            .HasOne(m => m.Remitente)
            .WithMany()
            .HasForeignKey(m => m.RemitenteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
