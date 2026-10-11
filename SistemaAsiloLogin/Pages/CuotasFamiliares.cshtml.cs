using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    public class CuotasFamiliaresModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public CuotasFamiliaresModel(AsiloDbContext context)
        {
            _context = context;
        }

        public List<Paciente> Pacientes { get; set; } = new();

        public List<CuotaFamiliar> Cuotas { get; set; } = new();

        [BindProperty]
        public CuotaFamiliar Cuota { get; set; } = new();

        private void CargarDatos()
        {
            Pacientes = _context.Pacientes
                .OrderBy(p => p.Nombres)
                .ToList();

            Cuotas = _context.CuotasFamiliares
                .Include(c => c.Paciente)
                .OrderByDescending(c => c.IdCuota)
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

            _context.CuotasFamiliares.Add(Cuota);
            _context.SaveChanges();

            return RedirectToPage();
        }
    }
}