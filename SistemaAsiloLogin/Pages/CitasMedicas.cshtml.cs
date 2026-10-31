using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador,MedicoGeneral,Medico")]
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
                 .Include(s => s.MedicoGeneral)
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
            // El médico y el estado no deben venir del formulario.
            ModelState.Remove("Cita.IdMedico");
            ModelState.Remove("Cita.Estado");

            var solicitud = _context.SolicitudesMedicas
                .Include(s => s.MedicoGeneral)
                .FirstOrDefault(s => s.IdSolicitud == Cita.IdSolicitud);

            if (solicitud == null)
            {
                ModelState.AddModelError(
                    "Cita.IdSolicitud",
                    "Debe seleccionar una solicitud médica válida.");
            }
            else if (solicitud.MedicoGeneral == null ||
                     !solicitud.MedicoGeneral.Estado)
            {
                ModelState.AddModelError(
                    "Cita.IdSolicitud",
                    "El médico asignado a esta solicitud no está activo.");
            }
            else
            {
                // El médico siempre se obtiene de la solicitud.
                Cita.IdMedico = solicitud.IdMedicoGeneral;
            }

            // Toda cita nueva comienza como Programada.
            Cita.Estado = "Programada";

            if (!ModelState.IsValid)
            {
                CargarDatos();
                return Page();
            }

            _context.CitasMedicas.Add(Cita);
            _context.SaveChanges();

            return RedirectToPage();
        }
    }
}