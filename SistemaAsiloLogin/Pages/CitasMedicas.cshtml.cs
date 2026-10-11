using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    public class CitasMedicasModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public CitasMedicasModel(AsiloDbContext context)
        {
            _context = context;
        }

        public List<SolicitudMedica> Solicitudes { get; set; } = new();

        public List<Medico> Medicos { get; set; } = new();

        public List<CitaMedica> Citas { get; set; } = new();

        [BindProperty]
        public CitaMedica Cita { get; set; } = new();

        private void CargarDatos()
        {
            Solicitudes = _context.SolicitudesMedicas
                .Include(s => s.Paciente)
                .Include(s => s.Especialidad)
                .ToList();

            Medicos = _context.Medicos
                .Where(m => m.Estado)
                .ToList();

            Citas = _context.CitasMedicas
                .Include(c => c.SolicitudMedica)
                    .ThenInclude(s => s!.Paciente)
                .Include(c => c.Medico)
                .OrderByDescending(c => c.IdCita)
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

            _context.CitasMedicas.Add(Cita);
            _context.SaveChanges();

            return RedirectToPage();
        }
    }
}