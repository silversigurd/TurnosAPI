using Microsoft.EntityFrameworkCore;
using TurnosAPI.Models;

namespace TurnosAPI.Data;

public class TurnosDbContext : DbContext
{
    public TurnosDbContext(DbContextOptions<TurnosDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Profesional> Profesionales => Set<Profesional>();
    public DbSet<Turno> Turnos => Set<Turno>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Telefono).HasMaxLength(20);
            entity.Property(c => c.Email).HasMaxLength(150);
        });

        modelBuilder.Entity<Profesional>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Especialidad).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Turno>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Estado).IsRequired();

            // Restrict: no se puede borrar un cliente o profesional si tiene turnos asociados
            entity.HasOne(t => t.Cliente)
                .WithMany()
                .HasForeignKey(t => t.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Profesional)
                .WithMany()
                .HasForeignKey(t => t.ProfesionalId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
