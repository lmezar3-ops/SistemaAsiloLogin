using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
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
            CargarDatos();
        }

        public IActionResult OnPost()
        {
            CargarDatos();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.ExamenesLaboratorio.Add(Examen);
            _context.SaveChanges();

            return RedirectToPage();
        }
    }
}