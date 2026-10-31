
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador")]
    public class ReportePagosFundacionModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public ReportePagosFundacionModel(
            AsiloDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? IdFundacion { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaDesde { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaHasta { get; set; }

        public List<Fundacion> Fundaciones { get; set; }
            = new();

        public List<PagoFundacion> Pagos { get; set; }
            = new();

        public decimal TotalRegistrado { get; set; }

        public decimal TotalPagado { get; set; }

        public decimal TotalOtrosEstados { get; set; }

        public int CantidadPagos { get; set; }

        public DateTime FechaEmision { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            DateTime hoy = DateTime.Today;

            FechaDesde ??= new DateTime(
                hoy.Year,
                hoy.Month,
                1
            );

            FechaHasta ??= hoy;

            FechaEmision = DateTime.Now;

            Fundaciones = await _context.Fundaciones
                .AsNoTracking()
                .OrderBy(f => f.Nombre)
                .ToListAsync();

            if (FechaDesde.Value.Date >
                FechaHasta.Value.Date)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "La fecha inicial no puede ser posterior a la fecha final."
                );

                return Page();
            }

            if (IdFundacion.HasValue &&
                !Fundaciones.Any(f =>
                    f.IdFundacion == IdFundacion.Value))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "La fundación seleccionada no existe."
                );

                return Page();
            }

            DateTime inicio = FechaDesde.Value.Date;

            DateTime finExclusivo =
                FechaHasta.Value.Date.AddDays(1);

            var consulta = _context.PagosFundacion
                .AsNoTracking()
                .Include(p => p.Fundacion)
                .Where(p =>
                    p.FechaPago >= inicio &&
                    p.FechaPago < finExclusivo
                );

            if (IdFundacion.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.IdFundacion == IdFundacion.Value
                );
            }

            Pagos = await consulta
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.IdPagoFundacion)
                .ToListAsync();

            CantidadPagos = Pagos.Count;

            TotalRegistrado = Pagos.Sum(p => p.Monto);

            TotalPagado = Pagos
                .Where(p =>
                    string.Equals(
                        p.Estado?.Trim(),
                        "Pagado",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Sum(p => p.Monto);

            TotalOtrosEstados =
                TotalRegistrado - TotalPagado;

            return Page();
        }
    }
}
