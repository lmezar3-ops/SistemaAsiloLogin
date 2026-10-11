using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    public class PagosPacienteModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public PagosPacienteModel(AsiloDbContext context)
        {
            _context = context;
        }

        public List<PagoPaciente> Pagos { get; set; } = new();

        public List<CargoPaciente> CargosPendientes { get; set; } = new();

        public List<CuotaFamiliar> CuotasPendientes { get; set; } = new();

        [BindProperty]
        public PagoPaciente NuevoPago { get; set; } = new();

        public async Task OnGetAsync()
        {
            await CargarDatosAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Un pago debe estar relacionado con un cargo o con una cuota,
            // pero no con ambos al mismo tiempo.
            bool tieneCargo = NuevoPago.IdCargo.HasValue;
            bool tieneCuota = NuevoPago.IdCuota.HasValue;

            if (tieneCargo == tieneCuota)
            {
                ModelState.AddModelError(
                    "",
                    "El pago debe estar relacionado con un cargo o con una cuota familiar."
                );
            }

            if (NuevoPago.MontoPagado <= 0)
            {
                ModelState.AddModelError(
                    nameof(NuevoPago.MontoPagado),
                    "El monto del pago debe ser mayor que cero."
                );
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosAsync();
                return Page();
            }

            NuevoPago.FechaPago = NuevoPago.FechaPago == default
                ? DateTime.Now
                : NuevoPago.FechaPago;

            _context.PagosPaciente.Add(NuevoPago);

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "El pago se registró correctamente.";

            return RedirectToPage();
        }

        private async Task CargarDatosAsync()
        {
            Pagos = await _context.PagosPaciente
                .Include(p => p.CargoPaciente)
                .Include(p => p.CuotaFamiliar)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            CargosPendientes = await _context.CargosPaciente
                .Where(c => c.Estado == "Pendiente")
                .OrderByDescending(c => c.FechaCargo)
                .ToListAsync();

            CuotasPendientes = await _context.CuotasFamiliares
                .Where(c => c.Estado == "Pendiente")
                .OrderByDescending(c => c.FechaCuota)
                .ToListAsync();
        }
    }
}