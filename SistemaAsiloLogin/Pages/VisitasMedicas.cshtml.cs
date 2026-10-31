using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador,MedicoGeneral,Medico,Enfermero")]
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
                .Where(c => c.Estado == "Programada"
         && !_context.VisitasMedicas.Any(v => v.IdCita == c.IdCita))
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
            // Paciente y médico siempre se obtienen de la cita.
            ModelState.Remove("Visita.IdPaciente");
            ModelState.Remove("Visita.IdMedico");

            var cita = _context.CitasMedicas
                .Include(c => c.SolicitudMedica)
                    .ThenInclude(s => s!.Paciente)
                .Include(c => c.Medico)
                .FirstOrDefault(c => c.IdCita == Visita.IdCita);

            if (cita == null)
            {
                ModelState.AddModelError(
                    "Visita.IdCita",
                    "Debe seleccionar una cita médica válida.");
            }
            else if (cita.Estado != "Programada")
            {
                ModelState.AddModelError(
                    "Visita.IdCita",
                    "La cita seleccionada no está programada.");
            }
            else if (cita.SolicitudMedica?.Paciente == null ||
                     cita.Medico == null)
            {
                ModelState.AddModelError(
                    "Visita.IdCita",
                    "La cita no tiene un paciente o médico válido.");
            }
            else if (_context.VisitasMedicas.Any(v => v.IdCita == cita.IdCita))
            {
                ModelState.AddModelError(
                    "Visita.IdCita",
                    "Esta cita ya tiene una visita médica registrada.");
            }
            else
            {
                // Asignación automática desde la cita.
                Visita.IdPaciente = cita.SolicitudMedica.IdPaciente;
                Visita.IdMedico = cita.IdMedico;
            }

            if (Visita.FechaVisita == default)
            {
                ModelState.AddModelError(
                    "Visita.FechaVisita",
                    "Debe indicar la fecha y hora de la visita.");
            }

            if (!ModelState.IsValid)
            {
                CargarDatos();
                return Page();
            }

            // La cita pasa a Atendida al registrar su visita.
            cita!.Estado = "Atendida";

            _context.VisitasMedicas.Add(Visita);
            _context.SaveChanges();

            return RedirectToPage();
        }
    }
}