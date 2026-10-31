
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador")]
    public class ReporteCuotasMensualesModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public ReporteCuotasMensualesModel(AsiloDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? IdPaciente { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Periodo { get; set; }

        public List<Paciente> Pacientes { get; set; } = new();

        public List<FilaCuota> Detalles { get; set; } = new();

        public decimal TotalCuotas { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal TotalPendiente { get; set; }
        public decimal TotalExcedente { get; set; }

        public int CantidadCuotas { get; set; }
        public int CuotasPagadas { get; set; }
        public int CuotasParciales { get; set; }
        public int CuotasPendientes { get; set; }

        public DateTime FechaEmision { get; set; }
        public DateTime FechaCorte { get; set; }

        public string NombrePeriodo { get; set; } = "";

        public class FilaCuota
        {
            public int IdCuota { get; set; }
            public string Paciente { get; set; } = "";
            public DateTime FechaCuota { get; set; }
            public decimal Monto { get; set; }
            public decimal Pagado { get; set; }
            public decimal Pendiente { get; set; }
            public decimal Excedente { get; set; }
            public string EstadoRegistrado { get; set; } = "";
            public string EstadoCalculado { get; set; } = "";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var hoy = DateTime.Today;
            FechaEmision = DateTime.Now;

            Periodo ??= hoy.ToString("yyyy-MM");

            Pacientes = await _context.Pacientes
                .AsNoTracking()
                .OrderBy(p => p.Apellidos)
                .ThenBy(p => p.Nombres)
                .ToListAsync();

            if (!DateTime.TryParseExact(
                Periodo,
                "yyyy-MM",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var inicioMes))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Seleccione un mes y año válidos."
                );

                return Page();
            }

            if (IdPaciente.HasValue &&
                !Pacientes.Any(p =>
                    p.IdPaciente == IdPaciente.Value))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "El paciente seleccionado no existe."
                );

                return Page();
            }

            var inicio = new DateTime(
                inicioMes.Year,
                inicioMes.Month,
                1
            );

            var finExclusivo = inicio.AddMonths(1);

            FechaCorte = finExclusivo.AddTicks(-1);

            NombrePeriodo = inicio.ToString(
                "MMMM yyyy",
                new CultureInfo("es-GT")
            );

            var consulta = _context.CuotasFamiliares
                .AsNoTracking()
                .Include(c => c.Paciente)
                .Where(c =>
                    c.FechaCuota >= inicio &&
                    c.FechaCuota < finExclusivo
                );

            if (IdPaciente.HasValue)
            {
                consulta = consulta.Where(c =>
                    c.IdPaciente == IdPaciente.Value
                );
            }

            var cuotas = await consulta
                .OrderBy(c => c.FechaCuota)
                .ThenBy(c => c.IdCuota)
                .ToListAsync();

            var idsCuotas = cuotas
                .Select(c => c.IdCuota)
                .ToList();

            // Pagos de estas cuotas registrados hasta
            // el cierre del mes seleccionado.
            var pagos = await _context.PagosPaciente
                .AsNoTracking()
                .Where(p =>
                    p.IdCuota.HasValue &&
                    idsCuotas.Contains(p.IdCuota.Value) &&
                    p.FechaPago < finExclusivo
                )
                .GroupBy(p => p.IdCuota!.Value)
                .Select(g => new
                {
                    IdCuota = g.Key,
                    Total = g.Sum(p => p.MontoPagado)
                })
                .ToListAsync();

            var pagosPorCuota = pagos.ToDictionary(
                p => p.IdCuota,
                p => p.Total
            );

            foreach (var cuota in cuotas)
            {
                var pagado = pagosPorCuota.GetValueOrDefault(
                    cuota.IdCuota,
                    0m
                );

                var pendiente = Math.Max(
                    0m,
                    cuota.Monto - pagado
                );

                var excedente = Math.Max(
                    0m,
                    pagado - cuota.Monto
                );

                string estadoCalculado;

                if (pagado >= cuota.Monto)
                {
                    estadoCalculado = "Pagada";
                }
                else if (pagado > 0)
                {
                    estadoCalculado = "Parcial";
                }
                else
                {
                    estadoCalculado = "Pendiente";
                }

                Detalles.Add(new FilaCuota
                {
                    IdCuota = cuota.IdCuota,
                    Paciente = cuota.Paciente == null
                        ? $"Paciente #{cuota.IdPaciente}"
                        : $"{cuota.Paciente.Nombres} {cuota.Paciente.Apellidos}",
                    FechaCuota = cuota.FechaCuota,
                    Monto = cuota.Monto,
                    Pagado = pagado,
                    Pendiente = pendiente,
                    Excedente = excedente,
                    EstadoRegistrado = cuota.Estado,
                    EstadoCalculado = estadoCalculado
                });
            }

            CantidadCuotas = Detalles.Count;

            TotalCuotas = Detalles.Sum(c => c.Monto);
            TotalPagado = Detalles.Sum(c => c.Pagado);
            TotalPendiente = Detalles.Sum(c => c.Pendiente);
            TotalExcedente = Detalles.Sum(c => c.Excedente);

            CuotasPagadas = Detalles.Count(c =>
                c.EstadoCalculado == "Pagada");

            CuotasParciales = Detalles.Count(c =>
                c.EstadoCalculado == "Parcial");

            CuotasPendientes = Detalles.Count(c =>
                c.EstadoCalculado == "Pendiente");

            return Page();
        }
    }
}
