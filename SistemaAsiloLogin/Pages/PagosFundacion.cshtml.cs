
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador")]
    public class PagosFundacionModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public PagosFundacionModel(AsiloDbContext context)
        {
            _context = context;
        }

        public List<PagoFundacion> ListaPagos { get; set; } = new();

        public List<Fundacion> Fundaciones { get; set; } = new();

        [BindProperty]
        public PagoFundacion Pago { get; set; } = new();

        public decimal TotalPagos { get; set; }

        public async Task OnGetAsync()
        {
            Pago.FechaPago = DateTime.Today;
            await CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            Fundaciones = await _context.Set<Fundacion>()
                .AsNoTracking()
                .Where(f => f.Estado)
                .OrderBy(f => f.Nombre)
                .ToListAsync();

            ListaPagos = await _context.PagosFundacion
                .AsNoTracking()
                .Include(p => p.Fundacion)
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.IdPagoFundacion)
                .ToListAsync();

            TotalPagos = ListaPagos
                .Where(p => p.Estado == "Pagado")
                .Sum(p => p.Monto);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Pago.IdFundacion <= 0 ||
                !await _context.Set<Fundacion>()
                    .AnyAsync(f =>
                        f.IdFundacion == Pago.IdFundacion &&
                        f.Estado))
            {
                ModelState.AddModelError(
                    "Pago.IdFundacion",
                    "Selecciona una fundación activa.");
            }

            if (Pago.Monto <= 0)
            {
                ModelState.AddModelError(
                    "Pago.Monto",
                    "El monto debe ser mayor que cero.");
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosAsync();
                return Page();
            }

            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var fundacion = await _context.Set<Fundacion>()
                    .FirstAsync(f =>
                        f.IdFundacion == Pago.IdFundacion &&
                        f.Estado);

                Pago.FechaPago = Pago.FechaPago == default
                    ? DateTime.Today
                    : Pago.FechaPago.Date;

                Pago.Descripcion =
                    string.IsNullOrWhiteSpace(Pago.Descripcion)
                        ? null
                        : Pago.Descripcion.Trim();

                Pago.Estado = "Pagado";

                _context.PagosFundacion.Add(Pago);
                await _context.SaveChangesAsync();

                var movimiento = new MovimientoCaja
                {
                    FechaMovimiento = DateTime.Now,
                    TipoMovimiento = "Egreso",
                    Concepto =
                        $"Pago a fundación: {fundacion.Nombre} " +
                        $"(Pago #{Pago.IdPagoFundacion})",
                    Monto = Pago.Monto,
                    Observaciones = Pago.Descripcion,
                    Estado = "Registrado"
                };

                _context.MovimientosCaja.Add(movimiento);
                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                TempData["Mensaje"] =
                    "Pago a la fundación registrado correctamente " +
                    "y reflejado en Caja.";

                return RedirectToPage();
            }
            catch
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo registrar el pago. " +
                    "No se confirmaron los cambios.");

                await CargarDatosAsync();
                return Page();
            }
        }
    }
}
