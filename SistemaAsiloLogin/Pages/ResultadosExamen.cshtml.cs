using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
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
            Examenes = _context.ExamenesLaboratorio
                .Include(e => e.VisitaMedica)
                .OrderByDescending(e => e.IdExamen)
                .ToList();

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

            _context.ResultadosExamen.Add(Resultado);
            _context.SaveChanges();

            return RedirectToPage();
        }
    }
}