using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador")]
    public class ReporteExamenesMedicamentosModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public ReporteExamenesMedicamentosModel(
            AsiloDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? IdPaciente { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaDesde { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaHasta { get; set; }

        public List<Paciente> Pacientes { get; set; } = new();

        public Paciente? PacienteSeleccionado { get; set; }

        public List<FilaExamen> Examenes { get; set; } = new();

        public List<FilaTratamiento> Tratamientos { get; set; } = new();

        public int TotalExamenes => Examenes.Count;

        public int TotalConResultado =>
            Examenes.Count(e => e.Resultado != null);

        public int TotalTratamientos => Tratamientos.Count;

        public int TotalEntregados =>
            Tratamientos.Count(t => t.Entregado);

        public DateTime FechaEmision { get; set; }

        public class FilaExamen
        {
            public int IdExamen { get; set; }
            public string Paciente { get; set; } = "";
            public DateTime FechaSolicitud { get; set; }
            public string NombreExamen { get; set; } = "";
            public string Estado { get; set; } = "";
            public ResultadoExamen? Resultado { get; set; }
        }

        public class FilaTratamiento
        {
            public int IdTratamiento { get; set; }
            public string Paciente { get; set; } = "";
            public DateTime FechaVisita { get; set; }
            public int IdMedicamento { get; set; }
            public string Dosis { get; set; } = "";
            public string Frecuencia { get; set; } = "";
            public string Duracion { get; set; } = "";
            public int Cantidad { get; set; }
            public string Indicaciones { get; set; } = "";
            public string EstadoEntrega { get; set; } = "";

            public bool Entregado =>
                EstadoEntrega.Equals(
                    "ENTREGADO",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var hoy = DateTime.Today;

            FechaDesde ??= new DateTime(
                hoy.Year, hoy.Month, 1);

            FechaHasta ??= hoy;

            FechaEmision = DateTime.Now;

            Pacientes = await _context.Pacientes
                .AsNoTracking()
                .OrderBy(p => p.Apellidos)
                .ThenBy(p => p.Nombres)
                .ToListAsync();

            if (!ModelState.IsValid)
                return Page();

            if (FechaDesde.Value.Date > FechaHasta.Value.Date ||
                FechaHasta.Value.Date == DateTime.MaxValue.Date)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Seleccione un rango de fechas válido."
                );

                return Page();
            }

            if (IdPaciente.HasValue)
            {
                PacienteSeleccionado = Pacientes
                    .FirstOrDefault(p =>
                        p.IdPaciente == IdPaciente.Value);

                if (PacienteSeleccionado == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "El paciente seleccionado no existe."
                    );

                    return Page();
                }
            }

            var inicio = FechaDesde.Value.Date;
            var finExclusivo = FechaHasta.Value.Date.AddDays(1);

            // Exámenes: filtrados por fecha de solicitud.
            var consultaExamenes = _context.ExamenesLaboratorio
                .AsNoTracking()
                .Include(e => e.VisitaMedica)
                    .ThenInclude(v => v!.Paciente)
                .Where(e =>
                    e.FechaSolicitud >= inicio &&
                    e.FechaSolicitud < finExclusivo);

            if (IdPaciente.HasValue)
            {
                consultaExamenes = consultaExamenes.Where(e =>
                    e.VisitaMedica != null &&
                    e.VisitaMedica.IdPaciente == IdPaciente.Value);
            }

            var examenesBD = await consultaExamenes
                .OrderByDescending(e => e.FechaSolicitud)
                .ThenByDescending(e => e.IdExamen)
                .ToListAsync();

            var idsExamenes = examenesBD
                .Select(e => e.IdExamen)
                .ToList();

            // Resultados disponibles actualmente para
            // los exámenes encontrados.
            var resultados = await _context.ResultadosExamen
                .AsNoTracking()
                .Where(r => idsExamenes.Contains(r.IdExamen))
                .OrderByDescending(r => r.FechaResultado)
                .ThenByDescending(r => r.IdResultado)
                .ToListAsync();

            var resultadoPorExamen = resultados
                .GroupBy(r => r.IdExamen)
                .ToDictionary(
                    g => g.Key,
                    g => g.First()
                );

            foreach (var examen in examenesBD)
            {
                var paciente = examen.VisitaMedica?.Paciente;

                resultadoPorExamen.TryGetValue(
                    examen.IdExamen,
                    out var resultado
                );

                Examenes.Add(new FilaExamen
                {
                    IdExamen = examen.IdExamen,
                    Paciente = paciente == null
                        ? "Sin información"
                        : $"{paciente.Nombres} {paciente.Apellidos}",
                    FechaSolicitud = examen.FechaSolicitud,
                    NombreExamen = examen.NombreExamen,
                    Estado = examen.Estado,
                    Resultado = resultado
                });
            }

            // Tratamientos: filtrados por fecha
            // de la visita médica asociada.
            var consultaTratamientos = _context.Tratamientos
                .AsNoTracking()
                .Include(t => t.VisitaMedica)
                    .ThenInclude(v => v!.Paciente)
                .Where(t =>
                    t.VisitaMedica != null &&
                    t.VisitaMedica.FechaVisita >= inicio &&
                    t.VisitaMedica.FechaVisita < finExclusivo);

            if (IdPaciente.HasValue)
            {
                consultaTratamientos = consultaTratamientos.Where(t =>
                    t.VisitaMedica != null &&
                    t.VisitaMedica.IdPaciente == IdPaciente.Value);
            }

            var tratamientosBD = await consultaTratamientos
                .OrderByDescending(t =>
                    t.VisitaMedica!.FechaVisita)
                .ThenByDescending(t => t.IdTratamiento)
                .ToListAsync();

            foreach (var tratamiento in tratamientosBD)
            {
                var visita = tratamiento.VisitaMedica;
                var paciente = visita?.Paciente;

                Tratamientos.Add(new FilaTratamiento
                {
                    IdTratamiento = tratamiento.IdTratamiento,
                    Paciente = paciente == null
                        ? "Sin información"
                        : $"{paciente.Nombres} {paciente.Apellidos}",
                    FechaVisita = visita?.FechaVisita ?? default,
                    IdMedicamento = tratamiento.IdMedicamento,
                    Dosis = tratamiento.Dosis,
                    Frecuencia = tratamiento.Frecuencia,
                    Duracion = tratamiento.Duracion,
                    Cantidad = tratamiento.Cantidad,
                    Indicaciones = tratamiento.Indicaciones,
                    EstadoEntrega = tratamiento.EstadoEntrega
                        ?? "Sin confirmar"
                });
            }

            return Page();
        }
    }
}
