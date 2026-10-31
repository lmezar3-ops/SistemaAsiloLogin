
using Microsoft.EntityFrameworkCore;
using MicroservicioMedicamentos.Models;

namespace MicroservicioMedicamentos.Data
{
    public class MedicamentosDbContext : DbContext
    {
        public MedicamentosDbContext(
            DbContextOptions<MedicamentosDbContext> options)
            : base(options)
        {
        }

        public DbSet<Medicamento> Medicamentos { get; set; }

        public DbSet<EntregaMedicamento> EntregasMedicamento { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Medicamento>(entity =>
            {
                entity.ToTable("Medicamento");

                entity.HasKey(m => m.IdMedicamento);

                entity.Property(m => m.IdMedicamento)
                    .HasColumnName("idMedicamento")
                    .ValueGeneratedOnAdd();

                entity.Property(m => m.Nombre)
                    .HasColumnName("nombre")
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .IsRequired();

                entity.Property(m => m.Descripcion)
                    .HasColumnName("descripcion")
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(m => m.Existencia)
                    .HasColumnName("existencia");

                entity.Property(m => m.FechaVencimiento)
                    .HasColumnName("fechaVencimiento")
                    .HasColumnType("date");

                entity.Property(m => m.Costo)
                    .HasColumnName("costo")
                    .HasPrecision(10, 2);

                entity.Property(m => m.Estado)
                    .HasColumnName("estado");
            });

            modelBuilder.Entity<EntregaMedicamento>(entity =>
            {
                entity.ToTable("EntregaMedicamento");

                entity.HasKey(e => e.IdEntrega);

                entity.Property(e => e.IdEntrega)
                    .HasColumnName("IdEntrega")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdTratamiento)
                    .HasColumnName("IdTratamiento");

                entity.Property(e => e.IdMedicamento)
                    .HasColumnName("IdMedicamento");

                entity.Property(e => e.Cantidad)
                    .HasColumnName("Cantidad");

                entity.Property(e => e.FechaEntrega)
                    .HasColumnName("FechaEntrega")
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("SYSDATETIME()");

                entity.HasIndex(e => e.IdTratamiento)
                    .IsUnique()
                    .HasDatabaseName("UQ_EntregaMedicamento_Tratamiento");

                entity.HasOne(e => e.Medicamento)
                    .WithMany()
                    .HasForeignKey(e => e.IdMedicamento)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}