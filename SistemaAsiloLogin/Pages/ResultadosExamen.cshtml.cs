
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador,MedicoGeneral,Medico,Laboratorio")]
    public class ResultadosExamenModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public ResultadosExamenModel(AsiloDbContext context)
        {
            _context = context;
        }

        public List<ExamenLaboratorio> Examenes { get; set; } = new();

        public List<ResultadoExamen> Resultados { get; set; } = new();

        [BindProperty]
        public ResultadoExamen Resultado { get; set; } = new();

        private void CargarDatos()
        {
            // Solo mostrar exámenes pendientes que aún no tengan resultado.
            Examenes = _context.ExamenesLaboratorio
                .Where(e =>
                    e.Estado != "Cancelado" &&
                    !_context.ResultadosExamen
                        .Any(r => r.IdExamen == e.IdExamen))
                .Include(e => e.VisitaMedica)
                .OrderByDescending(e => e.IdExamen)
                .ToList();

            // Conservar y mostrar todos los resultados registrados.
            Resultados = _context.ResultadosExamen
                .Include(r => r.ExamenLaboratorio)
                .OrderByDescending(r => r.IdResultado)
                .ToList();
        }

        public void OnGet()
        {
            CargarDatos();
        }

        public IActionResult OnPost()
        {
            CargarDatos();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var examen = _context.ExamenesLaboratorio
                .FirstOrDefault(e => e.IdExamen == Resultado.IdExamen);

            if (examen == null)
            {
                TempData["Error"] = "El examen seleccionado no existe.";
                return RedirectToPage();
            }

            if (string.Equals(
                examen.Estado,
                "Cancelado",
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] =
                    "No se puede registrar un resultado para un examen cancelado.";

                return RedirectToPage();
            }

            bool yaTieneResultado = _context.ResultadosExamen
                .Any(r => r.IdExamen == Resultado.IdExamen);

            if (yaTieneResultado)
            {
                TempData["Error"] =
                    "Este examen ya tiene un resultado registrado. No se permiten duplicados.";

                return RedirectToPage();
            }

            _context.ResultadosExamen.Add(Resultado);
            examen.Estado = "Realizado";

            _context.SaveChanges();

            TempData["Mensaje"] =
                "Resultado registrado correctamente. El examen ahora figura como Realizado.";

            return RedirectToPage();
        }
    }
}
