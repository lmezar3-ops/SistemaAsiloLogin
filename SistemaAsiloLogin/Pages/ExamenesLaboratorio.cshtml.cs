
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador,MedicoGeneral,Medico,Laboratorio")]
    public class ExamenesLaboratorioModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public ExamenesLaboratorioModel(AsiloDbContext context)
        {
            _context = context;
        }

        public List<VisitaMedica> Visitas { get; set; } = new();
        public List<ExamenLaboratorio> Examenes { get; set; } = new();

        [BindProperty]
        public ExamenLaboratorio Examen { get; set; } = new();

        public string? AdvertenciaRepeticion { get; set; }

        private void CargarDatos()
        {
            Visitas = _context.VisitasMedicas
                .Include(v => v.Paciente)
                .Include(v => v.Medico)
                .OrderByDescending(v => v.IdVisita)
                .ToList();

            Examenes = _context.ExamenesLaboratorio
                .Include(e => e.VisitaMedica)
                .OrderByDescending(e => e.IdExamen)
                .ToList();
        }

        public void OnGet()
        {
            var ahora = DateTime.Now;

            Examen.FechaSolicitud = new DateTime(
                ahora.Year,
                ahora.Month,
                ahora.Day,
                ahora.Hour,
                ahora.Minute,
                0
            );

            CargarDatos();
        }

        public IActionResult OnPost()
        {
            CargarDatos();

            // Validar que la visita seleccionada exista.
            var visita = _context.VisitasMedicas
                .FirstOrDefault(v => v.IdVisita == Examen.IdVisita);

            if (visita == null)
            {
                ModelState.AddModelError(
                    "Examen.IdVisita",
                    "Selecciona una visita médica válida.");

                return Page();
            }

            if (string.IsNullOrWhiteSpace(Examen.NombreExamen))
            {
                ModelState.AddModelError(
                    "Examen.NombreExamen",
                    "Escribe el nombre del examen.");

                return Page();
            }

            if (Examen.NombreExamen.Trim().Length > 150)
            {
                ModelState.AddModelError(
                    "Examen.NombreExamen",
                    "El nombre del examen no puede superar 150 caracteres.");

                return Page();
            }

            if (Examen.FechaSolicitud == default)
            {
                ModelState.AddModelError(
                    "Examen.FechaSolicitud",
                    "Debe indicar una fecha de solicitud válida.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Examen.NombreExamen = Examen.NombreExamen.Trim();

            // Buscar el mismo tipo de examen del mismo paciente
            // en los últimos 7 días, incluso si tuvo otra visita.
            DateTime fechaDesde = Examen.FechaSolicitud.AddDays(-7);
            DateTime fechaHasta = Examen.FechaSolicitud;

            bool examenRepetido = _context.ExamenesLaboratorio
                .Include(e => e.VisitaMedica)
                .Any(e =>
                    e.VisitaMedica != null &&
                    e.VisitaMedica.IdPaciente == visita.IdPaciente &&
                    e.NombreExamen.Trim().ToLower() ==
                        Examen.NombreExamen.ToLower() &&
                    e.FechaSolicitud >= fechaDesde &&
                    e.FechaSolicitud <= fechaHasta &&
                    e.Estado != "Cancelado");

            if (examenRepetido &&
                string.IsNullOrWhiteSpace(Examen.JustificacionRepeticion))
            {
                AdvertenciaRepeticion =
                    "Este paciente tiene un examen del mismo tipo solicitado " +
                    "en un intervalo de 7 días. Si es necesario repetirlo, " +
                    "escribe la justificación médica para continuar.";

                return Page();
            }

            if (Examen.JustificacionRepeticion?.Length > 500)
            {
                ModelState.AddModelError(
                    "Examen.JustificacionRepeticion",
                    "La justificación no puede superar 500 caracteres.");

                return Page();
            }

            // No aceptar datos de estado ni de cancelación enviados desde el formulario.
            Examen.Estado = "Pendiente";
            Examen.MotivoCancelacion = null;

            if (!examenRepetido)
            {
                Examen.JustificacionRepeticion = null;
            }
            else
            {
                Examen.JustificacionRepeticion =
                    Examen.JustificacionRepeticion?.Trim();
            }

            _context.ExamenesLaboratorio.Add(Examen);
            _context.SaveChanges();

            TempData["Mensaje"] = "Examen registrado correctamente.";
            return RedirectToPage();
        }

        public IActionResult OnPostCancelar(int id)
        {
            var examen = _context.ExamenesLaboratorio
                .FirstOrDefault(e => e.IdExamen == id);

            if (examen == null)
            {
                TempData["Error"] =
                    "No se encontró el examen que deseas cancelar.";

                return RedirectToPage();
            }

            if (string.Equals(
                examen.Estado,
                "Cancelado",
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Este examen ya está cancelado.";
                return RedirectToPage();
            }

            bool tieneResultado = _context.ResultadosExamen
                .Any(r => r.IdExamen == id);

            if (tieneResultado)
            {
                TempData["Error"] =
                    "No se puede cancelar este examen porque ya tiene un resultado registrado.";

                return RedirectToPage();
            }

            examen.Estado = "Cancelado";
            _context.SaveChanges();

            TempData["Mensaje"] =
                $"El examen #{id} fue cancelado. El registro se conservó.";

            return RedirectToPage();
        }
    }
}
