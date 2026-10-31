
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;
using System.Net.Http.Json;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador")]
    public class HistorialClinicoModel : PageModel
    {
        private readonly AsiloDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public HistorialClinicoModel(
            AsiloDbContext context,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public int? IdPaciente { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime FechaDesde { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime FechaHasta { get; set; }

        public List<Paciente> Pacientes { get; set; } = new();

        public Paciente? PacienteSeleccionado { get; set; }

        public List<VisitaMedica> Visitas { get; set; } = new();

        public List<ExamenLaboratorio> Examenes { get; set; } = new();

        public List<ResultadoExamen> Resultados { get; set; } = new();

        public List<Tratamiento> Tratamientos { get; set; } = new();

        public Dictionary<int, string> NombresMedicamentos { get; set; } = new();

        public class MedicamentoHistorialDto
        {
            public int IdMedicamento { get; set; }
            public string Nombre { get; set; } = string.Empty;
        }

        private async Task CargarNombresMedicamentosAsync()
        {
            NombresMedicamentos = new();

            if (Tratamientos.Count == 0)
                return;

            try
            {
                var cliente = _httpClientFactory.CreateClient("Medicamentos");
                cliente.Timeout = TimeSpan.FromSeconds(10);

                using var respuesta = await cliente.GetAsync(
                    "api/Medicamentos");

                if (!respuesta.IsSuccessStatusCode)
                    return;

                var medicamentos = await respuesta.Content
                    .ReadFromJsonAsync<List<MedicamentoHistorialDto>>();

                if (medicamentos == null)
                    return;

                NombresMedicamentos = medicamentos
                    .Where(m =>
                        m.IdMedicamento > 0 &&
                        !string.IsNullOrWhiteSpace(m.Nombre))
                    .GroupBy(m => m.IdMedicamento)
                    .ToDictionary(
                        grupo => grupo.Key,
                        grupo => grupo.First().Nombre);
            }
            catch (HttpRequestException)
            {
                // El historial sigue disponible aunque falle farmacia.
            }
            catch (TaskCanceledException)
            {
                // El historial sigue disponible si la consulta tarda demasiado.
            }
            catch (System.Text.Json.JsonException)
            {
                // El historial sigue disponible si el catálogo devuelve datos inválidos.
            }
        }

        public DateTime FechaEmision { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var hoy = DateTime.Today;

            if (FechaDesde == default)
                FechaDesde = new DateTime(hoy.Year, hoy.Month, 1);

            if (FechaHasta == default)
                FechaHasta = hoy;

            FechaEmision = DateTime.Now;

            Pacientes = await _context.Pacientes
                .AsNoTracking()
                .OrderBy(p => p.Nombres)
                .ThenBy(p => p.Apellidos)
                .ToListAsync();

            if (!IdPaciente.HasValue)
                return Page();

            PacienteSeleccionado = Pacientes
                .FirstOrDefault(p => p.IdPaciente == IdPaciente.Value);

            if (PacienteSeleccionado == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "El paciente seleccionado no existe.");

                return Page();
            }

            if (FechaDesde.Date > FechaHasta.Date)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "La fecha inicial no puede ser posterior a la fecha final.");

                return Page();
            }

            var inicio = FechaDesde.Date;
            var finExclusivo = FechaHasta.Date.AddDays(1);

            Visitas = await _context.VisitasMedicas
                .AsNoTracking()
                .Include(v => v.Medico)
                .Where(v =>
                    v.IdPaciente == IdPaciente.Value &&
                    v.FechaVisita >= inicio &&
                    v.FechaVisita < finExclusivo)
                .OrderByDescending(v => v.FechaVisita)
                .ToListAsync();

            var idsVisitas = Visitas
                .Select(v => v.IdVisita)
                .ToList();

            Examenes = await _context.ExamenesLaboratorio
                .AsNoTracking()
                .Where(e =>
                    idsVisitas.Contains(e.IdVisita) &&
                    e.FechaSolicitud >= inicio &&
                    e.FechaSolicitud < finExclusivo)
                .OrderByDescending(e => e.FechaSolicitud)
                .ToListAsync();

            var idsExamenes = Examenes
                .Select(e => e.IdExamen)
                .ToList();

            Resultados = await _context.ResultadosExamen
                .AsNoTracking()
                .Where(r =>
                    idsExamenes.Contains(r.IdExamen) &&
                    r.FechaResultado >= inicio &&
                    r.FechaResultado < finExclusivo)
                .OrderByDescending(r => r.FechaResultado)
                .ToListAsync();

            Tratamientos = await _context.Tratamientos
                .AsNoTracking()
                .Where(t =>
                    idsVisitas.Contains(t.IdVisita))
                .OrderByDescending(t => t.IdTratamiento)
                .ToListAsync();

            await CargarNombresMedicamentosAsync();

            return Page();
        }
    }
}
