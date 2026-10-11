
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
        }
    }
}