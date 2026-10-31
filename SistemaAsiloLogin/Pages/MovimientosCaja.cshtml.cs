
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador")]
    public class MovimientosCajaModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public MovimientosCajaModel(AsiloDbContext context)
        {
            _context = context;
        }

        public List<MovimientoCaja> Movimientos { get; set; } = new();

        public decimal TotalIngresos { get; set; }

        public decimal TotalEgresos { get; set; }

        public decimal Saldo { get; set; }

        public async Task OnGetAsync()
        {
            Movimientos = await _context.MovimientosCaja
                .AsNoTracking()
                .OrderByDescending(m => m.FechaMovimiento)
                .ThenByDescending(m => m.IdMovimiento)
                .ToListAsync();

            TotalIngresos = Movimientos
                .Where(m => m.TipoMovimiento == "Ingreso"
                    && m.Estado == "Registrado")
                .Sum(m => m.Monto);

            TotalEgresos = Movimientos
                .Where(m => m.TipoMovimiento == "Egreso"
                    && m.Estado == "Registrado")
                .Sum(m => m.Monto);

            Saldo = TotalIngresos - TotalEgresos;
        }
    }
}
