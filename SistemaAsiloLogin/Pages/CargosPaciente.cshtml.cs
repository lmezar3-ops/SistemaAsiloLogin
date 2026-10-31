using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador,CajaFinanzas")]
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
            var ahora = DateTime.Now;

            Cargo.FechaCargo = new DateTime(
                ahora.Year,
                ahora.Month,
                ahora.Day,
                ahora.Hour,
                ahora.Minute,
                0
            );

            CargarDatos();
        }


        public IActionResult OnPost()
        {
            CargarDatos();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Validar que el paciente exista y esté activo.
            var pacienteExiste = _context.Pacientes.Any(p =>
                p.IdPaciente == Cargo.IdPaciente && p.Estado);

            if (!pacienteExiste)
            {
                ModelState.AddModelError(
                    "Cargo.IdPaciente",
                    "Selecciona un paciente activo.");

                return Page();
            }

            // Validar que la fundación exista y esté activa.
            var fundacionExiste = _context.Fundaciones.Any(f =>
                f.IdFundacion == Cargo.IdFundacion && f.Estado);

            if (!fundacionExiste)
            {
                ModelState.AddModelError(
                    "Cargo.IdFundacion",
                    "Selecciona una fundación activa.");

                return Page();
            }

            // Solo se aceptan estos tipos de cargo.
            var tiposPermitidos = new[] { "Consulta", "Examen", "Medicamento" };

            if (!tiposPermitidos.Contains(Cargo.TipoCargo))
            {
                ModelState.AddModelError(
                    "Cargo.TipoCargo",
                    "Selecciona un tipo de cargo válido.");

                return Page();
            }

            if (string.IsNullOrWhiteSpace(Cargo.Concepto) ||
                Cargo.Concepto.Trim().Length > 300)
            {
                ModelState.AddModelError(
                    "Cargo.Concepto",
                    "El concepto es obligatorio y no debe superar 200 caracteres.");

                return Page();
            }

            if (Cargo.MontoOriginal <= 0)
            {
                ModelState.AddModelError(
                    "Cargo.MontoOriginal",
                    "El monto original debe ser mayor que cero.");

                return Page();
            }

            // El servidor calcula el descuento; no confía en el navegador.
            Cargo.Descuento = Math.Round(
                Cargo.MontoOriginal * 0.20m,
                2,
                MidpointRounding.AwayFromZero);

            Cargo.MontoFinal = Cargo.MontoOriginal - Cargo.Descuento;

            Cargo.Concepto = Cargo.Concepto.Trim();
            Cargo.Estado = "Pendiente";

            if (Cargo.FechaCargo == default)
            {
                Cargo.FechaCargo = DateTime.Now;
            }

            _context.CargosPaciente.Add(Cargo);
            _context.SaveChanges();

            TempData["Mensaje"] =
                "Cargo registrado correctamente con descuento del 20 % y estado pendiente.";

            return RedirectToPage();
        }

    }
}