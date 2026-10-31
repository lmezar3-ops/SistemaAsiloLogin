
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador")]
    public class ReporteDonacionesModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public ReporteDonacionesModel(AsiloDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? IdDonante { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TipoDonacion { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaDesde { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaHasta { get; set; }

        public List<Donante> Donantes { get; set; } = new();

        public List<string> TiposDonacion { get; set; } = new();

        public List<Donacion> Donaciones { get; set; } = new();

        public List<ResumenTipoDonacion> ResumenTipos { get; set; }
            = new();

        public decimal TotalDonaciones { get; set; }

        public int CantidadDonaciones { get; set; }

        public int CantidadDonantes { get; set; }

        public DateTime FechaEmision { get; set; }

        public class ResumenTipoDonacion
        {
            public string Tipo { get; set; } = "";

            public int Cantidad { get; set; }

            public decimal Monto { get; set; }
        }

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

            Donantes = await _context.Donantes
                .AsNoTracking()
                .OrderBy(d => d.Nombre)
                .ToListAsync();

            TiposDonacion = await _context.Donaciones
                .AsNoTracking()
                .Select(d => d.TipoDonacion)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (FechaDesde.Value.Date > FechaHasta.Value.Date)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "La fecha inicial no puede ser posterior a la fecha final."
                );

                return Page();
            }

            if (IdDonante.HasValue &&
                !Donantes.Any(d => d.IdDonante == IdDonante.Value))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "El donante seleccionado no existe."
                );

                return Page();
            }

            if (!string.IsNullOrWhiteSpace(TipoDonacion) &&
                !TiposDonacion.Contains(TipoDonacion))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "El tipo de donación seleccionado no existe."
                );

                return Page();
            }

            DateTime inicio = FechaDesde.Value.Date;

            DateTime finExclusivo =
                FechaHasta.Value.Date.AddDays(1);

            var consulta = _context.Donaciones
                .AsNoTracking()
                .Include(d => d.Donante)
                .Where(d =>
                    d.FechaDonacion >= inicio &&
                    d.FechaDonacion < finExclusivo
                );

            if (IdDonante.HasValue)
            {
                consulta = consulta.Where(d =>
                    d.IdDonante == IdDonante.Value
                );
            }

            if (!string.IsNullOrWhiteSpace(TipoDonacion))
            {
                consulta = consulta.Where(d =>
                    d.TipoDonacion == TipoDonacion
                );
            }

            Donaciones = await consulta
                .OrderByDescending(d => d.FechaDonacion)
                .ThenByDescending(d => d.IdDonacion)
                .ToListAsync();

            CantidadDonaciones = Donaciones.Count;

            TotalDonaciones = Donaciones.Sum(d => d.Monto);

            CantidadDonantes = Donaciones
                .Select(d => d.IdDonante)
                .Distinct()
                .Count();

            ResumenTipos = Donaciones
                .GroupBy(d => d.TipoDonacion)
                .Select(g => new ResumenTipoDonacion
                {
                    Tipo = g.Key,
                    Cantidad = g.Count(),
                    Monto = g.Sum(d => d.Monto)
                })
                .OrderByDescending(r => r.Monto)
                .ToList();

            return Page();
        }
    }
}
