
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador,CajaFinanzas")]
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

        public Dictionary<int, decimal> SaldosCargosPendientes { get; set; } = new();

        [BindProperty]
        public PagoPaciente NuevoPago { get; set; } = new();

        public async Task OnGetAsync()
        {
            await CargarDatosAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool tieneCargo = NuevoPago.IdCargo.HasValue;
            bool tieneCuota = NuevoPago.IdCuota.HasValue;

            if (tieneCargo == tieneCuota)
            {
                ModelState.AddModelError(
                    "",
                    "Selecciona exactamente un cargo o una cuota familiar."
                );
            }

            if (NuevoPago.MontoPagado <= 0)
            {
                ModelState.AddModelError(
                    nameof(NuevoPago.MontoPagado),
                    "El monto debe ser mayor que cero."
                );
            }

            if (string.IsNullOrWhiteSpace(NuevoPago.MetodoPago))
            {
                ModelState.AddModelError(
                    nameof(NuevoPago.MetodoPago),
                    "Selecciona un método de pago."
                );
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosAsync();
                return Page();
            }

            string concepto;

            if (tieneCargo)
            {
                var cargo = await _context.CargosPaciente
                    .SingleOrDefaultAsync(c => c.IdCargo == NuevoPago.IdCargo);

                if (cargo == null)
                {
                    ModelState.AddModelError("", "El cargo seleccionado no existe.");
                    await CargarDatosAsync();
                    return Page();
                }

                if (cargo.Estado != "Pendiente")
                {
                    ModelState.AddModelError(
                        "",
                        "El cargo seleccionado ya no está pendiente."
                    );

                    await CargarDatosAsync();
                    return Page();
                }

                decimal totalPagado = await _context.PagosPaciente
                    .Where(p => p.IdCargo == cargo.IdCargo)
                    .SumAsync(p => (decimal?)p.MontoPagado) ?? 0m;

                decimal saldo = cargo.MontoFinal - totalPagado;

                if (saldo <= 0)
                {
                    cargo.Estado = "Pagado";

                    await _context.SaveChangesAsync();

                    ModelState.AddModelError(
                        "",
                        "El cargo ya está completamente pagado."
                    );

                    await CargarDatosAsync();
                    return Page();
                }

                if (NuevoPago.MontoPagado > saldo)
                {
                    ModelState.AddModelError(
                        nameof(NuevoPago.MontoPagado),
                        $"El pago supera el saldo pendiente de Q {saldo:N2}."
                    );

                    await CargarDatosAsync();
                    return Page();
                }

                decimal nuevoTotal = totalPagado + NuevoPago.MontoPagado;

                if (nuevoTotal >= cargo.MontoFinal)
                {
                    cargo.Estado = "Pagado";
                }

                concepto = $"Pago del cargo #{cargo.IdCargo}";
            }
            else
            {
                var cuota = await _context.CuotasFamiliares
                    .SingleOrDefaultAsync(c => c.IdCuota == NuevoPago.IdCuota);

                if (cuota == null)
                {
                    ModelState.AddModelError("", "La cuota seleccionada no existe.");
                    await CargarDatosAsync();
                    return Page();
                }

                if (cuota.Estado != "Pendiente")
                {
                    ModelState.AddModelError(
                        "",
                        "La cuota seleccionada ya no está pendiente."
                    );

                    await CargarDatosAsync();
                    return Page();
                }

                decimal totalPagado = await _context.PagosPaciente
                    .Where(p => p.IdCuota == cuota.IdCuota)
                    .SumAsync(p => (decimal?)p.MontoPagado) ?? 0m;

                decimal saldo = cuota.Monto - totalPagado;

                if (saldo <= 0)
                {
                    cuota.Estado = "Pagado";

                    await _context.SaveChangesAsync();

                    ModelState.AddModelError(
                        "",
                        "La cuota ya está completamente pagada."
                    );

                    await CargarDatosAsync();
                    return Page();
                }

                if (NuevoPago.MontoPagado > saldo)
                {
                    ModelState.AddModelError(
                        nameof(NuevoPago.MontoPagado),
                        $"El pago supera el saldo pendiente de Q {saldo:N2}."
                    );

                    await CargarDatosAsync();
                    return Page();
                }

                decimal nuevoTotal = totalPagado + NuevoPago.MontoPagado;

                if (nuevoTotal >= cuota.Monto)
                {
                    cuota.Estado = "Pagado";
                }

                concepto = $"Pago de cuota familiar #{cuota.IdCuota}";
            }

            NuevoPago.FechaPago = NuevoPago.FechaPago == default
                ? DateTime.Now
                : NuevoPago.FechaPago;

            NuevoPago.MetodoPago = NuevoPago.MetodoPago.Trim();

            NuevoPago.Observaciones =
                NuevoPago.Observaciones?.Trim() ?? "";

            // Un mismo SaveChanges guarda el pago, el estado actualizado
            // y el movimiento de caja dentro de la transacción de EF Core.
            _context.PagosPaciente.Add(NuevoPago);

            _context.MovimientosCaja.Add(new MovimientoCaja
            {
                FechaMovimiento = NuevoPago.FechaPago,
                TipoMovimiento = "Ingreso",
                Concepto = concepto,
                Monto = NuevoPago.MontoPagado,
                Observaciones =
                    $"Método: {NuevoPago.MetodoPago}. " +
                    NuevoPago.Observaciones,
                Estado = "Registrado"
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo registrar el pago. Verifica la base de datos e inténtalo nuevamente."
                );

                await CargarDatosAsync();
                return Page();
            }

            TempData["Mensaje"] =
                "El pago y su ingreso en caja se registraron correctamente.";

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

            SaldosCargosPendientes = new Dictionary<int, decimal>();

            if (CargosPendientes.Count > 0)
            {
                var idsCargos = CargosPendientes
                    .Select(c => c.IdCargo)
                    .ToList();

                var pagosPorCargo = await _context.PagosPaciente
                    .AsNoTracking()
                    .Where(p => p.IdCargo.HasValue
                        && idsCargos.Contains(p.IdCargo.Value))
                    .GroupBy(p => p.IdCargo!.Value)
                    .Select(g => new
                    {
                        IdCargo = g.Key,
                        TotalPagado = g.Sum(p => p.MontoPagado)
                    })
                    .ToDictionaryAsync(
                        x => x.IdCargo,
                        x => x.TotalPagado
                    );

                foreach (var cargo in CargosPendientes)
                {
                    decimal totalPagado = pagosPorCargo.TryGetValue(
                        cargo.IdCargo,
                        out var total
                    ) ? total : 0m;

                    SaldosCargosPendientes[cargo.IdCargo] =
                        cargo.MontoFinal - totalPagado;
                }
            }

            CuotasPendientes = await _context.CuotasFamiliares
                .Where(c => c.Estado == "Pendiente")
                .OrderByDescending(c => c.FechaCuota)
                .ToListAsync();
        }
    }
}
