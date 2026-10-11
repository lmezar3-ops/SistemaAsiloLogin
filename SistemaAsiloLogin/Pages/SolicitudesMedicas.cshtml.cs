using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    public class SolicitudesMedicasModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public SolicitudesMedicasModel(AsiloDbContext context)
        {
            _context = context;
        }

        public List<Paciente> Pacientes { get; set; } = new();

        public List<Medico> Medicos { get; set; } = new();

        public List<Especialidad> Especialidades { get; set; } = new();

        public List<Enfermero> Enfermeros { get; set; } = new();

        public List<SolicitudMedica> Solicitudes { get; set; } = new();

        //new
        [BindProperty]
        public SolicitudMedica Solicitud { get; set; } = new();

        private void CargarCombos()
        {
            Pacientes = _context.Pacientes
                .Where(p => p.Estado)
                .ToList();

            Medicos = _context.Medicos
                .Where(m => m.Estado)
                .ToList();

            Especialidades = _context.Especialidades
                .ToList();

            Enfermeros = _context.Enfermeros
                .Where(e => e.Estado)
                .ToList();
        }

        private void CargarSolicitudes()
        {
            Solicitudes = _context.SolicitudesMedicas
             .Include(s => s.Paciente)
             .Include(s => s.MedicoGeneral)
             .Include(s => s.Especialidad)
             .Include(s => s.Enfermero)
             .OrderByDescending(s => s.IdSolicitud)
             .ToList();
        }

        public void OnGet()
        {
            CargarCombos();
            CargarSolicitudes();
        }

    public IActionResult OnPost()
        {
            CargarCombos();

            if (!ModelState.IsValid)
            {
                CargarSolicitudes();
                return Page();
            }

            _context.SolicitudesMedicas.Add(Solicitud);
            _context.SaveChanges();

            return RedirectToPage();
        }
     }
  }

