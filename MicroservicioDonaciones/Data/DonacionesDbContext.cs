using Microsoft.EntityFrameworkCore;
using MicroservicioDonaciones.Models;

namespace MicroservicioDonaciones.Data
{
    public class DonacionesDbContext : DbContext
    {
        public DonacionesDbContext(
            DbContextOptions<DonacionesDbContext> options)
            : base(options)
        {
        }

        public DbSet<Donacion> Donaciones { get; set; }

        public DbSet<Donante> Donantes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Donante>()
                .ToTable("Donante");

            modelBuilder.Entity<Donacion>()
                .ToTable("Donacion");

            modelBuilder.Entity<Donacion>()
                .HasOne(d => d.Donante)
                .WithMany(d => d.Donaciones)
                .HasForeignKey(d => d.IdDonante)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}