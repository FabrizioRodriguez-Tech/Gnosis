using Microsoft.EntityFrameworkCore;
using Gnosis.Domain.Entities;

namespace Gnosis.Infrastructure;

public class GnosisDbContext(DbContextOptions<GnosisDbContext> options) : DbContext(options)
{
    public DbSet<Tarea> Tareas => Set<Tarea>();
    public DbSet<SesionEnfoque> SesionesEnfoque => Set<SesionEnfoque>();
    public DbSet<BloqueTiempo> BloquesTiempo => Set<BloqueTiempo>();

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

            // Opcional: se completa solo cuando la tarea pasa a IsCompletada = true
            entity.Property(t => t.FechaCompletada);

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

        // =========================================================================
        // CONFIGURACIÓN DE LA ENTIDAD: BloqueTiempo (Agenda / Schedule)
        // =========================================================================
        modelBuilder.Entity<BloqueTiempo>(entity =>
        {
            entity.ToTable("BloquesTiempo");
            entity.HasKey(b => b.Id);

            entity.Property(b => b.Titulo)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(b => b.Descripcion)
                .HasMaxLength(1000);

            entity.Property(b => b.Color)
                .HasMaxLength(20);

            entity.Property(b => b.FechaInicio)
                .IsRequired();

            entity.Property(b => b.FechaFin)
                .IsRequired();

            // Relación opcional con Tarea: si se borra la tarea, el bloque queda sin asignar (no se borra)
            entity.HasOne(b => b.Tarea)
                .WithMany()
                .HasForeignKey(b => b.TareaId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}