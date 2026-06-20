using Microsoft.EntityFrameworkCore;
using Gnosis.Domain.Entities;

namespace Gnosis.Infrastructure;

public class GnosisDbContext(DbContextOptions<GnosisDbContext> options) : DbContext(options)
{
    public DbSet<Tarea> Tareas => Set<Tarea>();
    public DbSet<SesionEnfoque> SesionesEnfoque => Set<SesionEnfoque>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================================================================
        // CONFIGURACIÓN DE LA ENTIDAD: Tarea
        // =========================================================================
        modelBuilder.Entity<Tarea>(entity =>
        {
            entity.ToTable("Tareas");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Titulo)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.Descripcion)
                .HasMaxLength(1000);

            entity.Property(t => t.FechaCreacion)
                .IsRequired();

            // Configuración de la estructura jerárquica (Autorreferencia)
            entity.HasOne(t => t.TareaPadre)
                .WithMany(t => t.Subtareas)
                .HasForeignKey(t => t.TareaPadreId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================================================================
        // CONFIGURACIÓN DE LA ENTIDAD: SesionEnfoque
        // =========================================================================
        modelBuilder.Entity<SesionEnfoque>(entity =>
        {
            entity.ToTable("SesionesEnfoque");
            entity.HasKey(s => s.Id);

            // Mapeamos únicamente las propiedades de auditoría/tiempo base que tiene tu objeto
            entity.Property(s => s.FechaInicio)
                .IsRequired();

            entity.Property(s => s.FechaFin)
                .IsRequired();
        });
    }
}