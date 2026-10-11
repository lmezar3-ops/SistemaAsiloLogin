using Microsoft.EntityFrameworkCore;
using MicroservicioAncianos.Models;

namespace MicroservicioAncianos.Data
{
    public class AsiloDbContext : DbContext
    {
        public AsiloDbContext(DbContextOptions<AsiloDbContext> options)
            : base(options)
        {
        }

        public DbSet<Paciente> Pacientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Paciente>().ToTable("Paciente");
        }
    }
}