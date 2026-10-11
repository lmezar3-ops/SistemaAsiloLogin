using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Data
{
    public class AsiloDbContext : DbContext
    {
        public AsiloDbContext(DbContextOptions<AsiloDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Paciente> Pacientes { get; set; }

        public DbSet<Familiar> Familiares { get; set; }

        public DbSet<PacienteFamiliar> PacientesFamiliares { get; set; }

        public DbSet<Especialidad> Especialidades { get; set; }

        public DbSet<Medico> Medicos { get; set; }

        public DbSet<Enfermero> Enfermeros { get; set; }

        public DbSet<SolicitudMedica> SolicitudesMedicas { get; set; }

        public DbSet<CitaMedica> CitasMedicas { get; set; }

        public DbSet<VisitaMedica> VisitasMedicas { get; set; }

        public DbSet<ExamenLaboratorio> ExamenesLaboratorio { get; set; }

        public DbSet<ResultadoExamen> ResultadosExamen { get; set; }

        public DbSet<Tratamiento> Tratamientos { get; set; }

        public DbSet<Fundacion> Fundaciones { get; set; }

        public DbSet<Donante> Donantes { get; set; }

        public DbSet<Donacion> Donaciones { get; set; }

        public DbSet<CargoPaciente> CargosPaciente { get; set; }

        public DbSet<PagoPaciente> PagosPaciente { get; set; }

        public DbSet<CuotaFamiliar> CuotasFamiliares { get; set; }

        public DbSet<PagoFundacion> PagosFundacion { get; set; }

        public DbSet<Gasto> Gastos { get; set; }

        public DbSet<MovimientoCaja> MovimientosCaja { get; set; }

        public DbSet<Notificacion> Notificaciones { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().ToTable("Usuario");

            modelBuilder.Entity<Paciente>().ToTable("Paciente");

            modelBuilder.Entity<PacienteFamiliar>().HasKey(pf => new { pf.IdPaciente, pf.IdFamiliar });

            modelBuilder.Entity<PacienteFamiliar>().HasOne(pf => pf.Paciente)
    .WithMany()
    .HasForeignKey(pf => pf.IdPaciente);

            modelBuilder.Entity<PacienteFamiliar>().HasOne(pf => pf.Familiar)
                .WithMany()
                .HasForeignKey(pf => pf.IdFamiliar);

            modelBuilder.Entity<Especialidad>().ToTable("Especialidad");

            modelBuilder.Entity<Medico>().ToTable("Medico");

            modelBuilder.Entity<Enfermero>().ToTable("Enfermero");

            modelBuilder.Entity<SolicitudMedica>().ToTable("SolicitudMedica");

            modelBuilder.Entity<CitaMedica>().ToTable("CitaMedica");

            modelBuilder.Entity<VisitaMedica>().ToTable("VisitaMedica");

            modelBuilder.Entity<ExamenLaboratorio>().ToTable("ExamenLaboratorio");

            modelBuilder.Entity<ResultadoExamen>().ToTable("ResultadoExamen");

            modelBuilder.Entity<Tratamiento>().ToTable("Tratamiento");

            modelBuilder.Entity<Fundacion>().ToTable("Fundacion");

            modelBuilder.Entity<Donante>().ToTable("Donante");

            modelBuilder.Entity<Donacion>().ToTable("Donacion");

            modelBuilder.Entity<CargoPaciente>().ToTable("CargoPaciente");

            modelBuilder.Entity<PagoPaciente>().ToTable("PagoPaciente");

            modelBuilder.Entity<PagoPaciente>().HasOne(p => p.CargoPaciente)
           .WithMany().HasForeignKey(p => p.IdCargo).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PagoPaciente>()
                .HasOne(p => p.CuotaFamiliar)
                .WithMany()
                .HasForeignKey(p => p.IdCuota)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PagoFundacion>().ToTable("PagoFundacion");

            modelBuilder.Entity<Gasto>().ToTable("Gasto");

            modelBuilder.Entity<CuotaFamiliar>().ToTable("CuotaFamiliar");

            modelBuilder.Entity<MovimientoCaja>().ToTable("MovimientoCaja");

            modelBuilder.Entity<Notificacion>().ToTable("Notificacion");

        }
    }
}