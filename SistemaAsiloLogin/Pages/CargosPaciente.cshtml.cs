using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    public class CargosPacienteModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public CargosPacienteModel(AsiloDbContext context)
        {
            _context = context;
        }

        public List<Paciente> Pacientes { get; set; } = new();

        public List<Fundacion> Fundaciones { get; set; } = new();

        public List<CargoPaciente> Cargos { get; set; } = new();

        [BindProperty]
        public CargoPaciente Cargo { get; set; } = new();

        private void CargarDatos()
        {
            Pacientes = _context.Pacientes
                .Where(p => p.Estado)
                .OrderBy(p => p.Nombres)
                .ToList();

            Fundaciones = _context.Fundaciones
                .Where(f => f.Estado)
                .OrderBy(f => f.Nombre)
                .ToList();

            Cargos = _context.CargosPaciente
                .Include(c => c.Paciente)
                .Include(c => c.Fundacion)
                .OrderByDescending(c => c.IdCargo)
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

            _context.CargosPaciente.Add(Cargo);
            _context.SaveChanges();

            return RedirectToPage();
        }
    }
}