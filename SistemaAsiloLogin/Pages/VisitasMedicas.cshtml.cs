using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    public class VisitasMedicasModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public VisitasMedicasModel(AsiloDbContext context)
        {
            _context = context;
        }

        public List<CitaMedica> Citas { get; set; } = new();

        public List<VisitaMedica> Visitas { get; set; } = new();

        [BindProperty]
        public VisitaMedica Visita { get; set; } = new();

        private void CargarDatos()
        {
            Citas = _context.CitasMedicas
                .Include(c => c.SolicitudMedica)
                    .ThenInclude(s => s!.Paciente)
                .Include(c => c.Medico)
                .Where(c => c.Estado == "Programada")
                .OrderBy(c => c.FechaCita)
                .ToList();

            Visitas = _context.VisitasMedicas
                .Include(v => v.Paciente)
                .Include(v => v.Medico)
                .Include(v => v.CitaMedica)
                .OrderByDescending(v => v.IdVisita)
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

            _context.VisitasMedicas.Add(Visita);
            _context.SaveChanges();

            return RedirectToPage();
        }
    }
}