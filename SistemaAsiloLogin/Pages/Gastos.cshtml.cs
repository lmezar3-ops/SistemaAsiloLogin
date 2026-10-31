
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador,CajaFinanzas")]
    public class GastosModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public GastosModel(AsiloDbContext context)
        {
            _context = context;
        }

        public List<Gasto> ListaGastos { get; set; } = new();

        [BindProperty]
        public Gasto Gasto { get; set; } = new();

        public decimal TotalGastos { get; set; }

        public async Task OnGetAsync()
        {
            await CargarGastosAsync();
        }

        private async Task CargarGastosAsync()
        {
            ListaGastos = await _context.Gastos
                .AsNoTracking()
                .OrderByDescending(g => g.FechaGasto)
                .ThenByDescending(g => g.IdGasto)
                .ToListAsync();

            TotalGastos = ListaGastos
                .Where(g => g.Estado == "Registrado")
                .Sum(g => g.Monto);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Gasto.Monto <= 0)
            {
                ModelState.AddModelError(
                    "Gasto.Monto",
                    "El monto debe ser mayor que cero.");
            }

            if (string.IsNullOrWhiteSpace(Gasto.TipoGasto))
            {
                ModelState.AddModelError(
                    "Gasto.TipoGasto",
                    "Selecciona el tipo de gasto.");
            }

            if (string.IsNullOrWhiteSpace(Gasto.Concepto))
            {
                ModelState.AddModelError(
                    "Gasto.Concepto",
                    "Escribe el concepto del gasto.");
            }

            if (!ModelState.IsValid)
            {
                await CargarGastosAsync();
                return Page();
            }

            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                Gasto.TipoGasto = Gasto.TipoGasto.Trim();
                Gasto.Concepto = Gasto.Concepto.Trim();
                Gasto.Observaciones =
                    string.IsNullOrWhiteSpace(Gasto.Observaciones)
                        ? null
                        : Gasto.Observaciones.Trim();

                Gasto.FechaGasto = Gasto.FechaGasto == default
                    ? DateTime.Today
                    : Gasto.FechaGasto.Date;

                Gasto.Estado = "Registrado";

                _context.Gastos.Add(Gasto);

                await _context.SaveChangesAsync();

                var movimiento = new MovimientoCaja
                {
                    FechaMovimiento = DateTime.Now,
                    TipoMovimiento = "Egreso",
                    Concepto = $"Gasto #{Gasto.IdGasto}: {Gasto.Concepto}",
                    Monto = Gasto.Monto,
                    Observaciones = Gasto.Observaciones,
                    Estado = "Registrado"
                };

                _context.MovimientosCaja.Add(movimiento);

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["Mensaje"] =
                    "Gasto registrado correctamente y reflejado en Caja.";

                return RedirectToPage();
            }
            catch
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo registrar el gasto. No se confirmaron los cambios.");

                await CargarGastosAsync();
                return Page();
            }
        }
    }
}
