using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages.Reportes
{
    [Authorize(Roles = "Administrador")]
    public class CobrosPacientesModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public CobrosPacientesModel(AsiloDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? IdPaciente { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime FechaDesde { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime FechaHasta { get; set; }

        public List<Paciente> Pacientes { get; set; } = new();

        public Paciente? PacienteSeleccionado { get; set; }

        public List<DetalleCobro> Detalles { get; set; } = new();

        public decimal TotalCargosPeriodo { get; set; }

        public decimal TotalCuotasPeriodo { get; set; }

        public decimal TotalPagosPeriodo { get; set; }

        public decimal SaldoPendienteAlCierre { get; set; }

        public DateTime FechaEmision { get; private set; }

        public async Task OnGetAsync()
        {
            var hoy = DateTime.Today;

            if (FechaDesde == default)
            {
                FechaDesde = new DateTime(hoy.Year, hoy.Month, 1);
            }

            if (FechaHasta == default)
            {
                FechaHasta = hoy;
            }

            FechaEmision = DateTime.Now;

            Pacientes = await _context.Pacientes
                .AsNoTracking()
                .OrderBy(p => p.Nombres)
                .ThenBy(p => p.Apellidos)
                .ToListAsync();

            if (IdPaciente.HasValue)
            {
                PacienteSeleccionado = Pacientes
                    .FirstOrDefault(p => p.IdPaciente == IdPaciente.Value);

                if (PacienteSeleccionado == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "El paciente seleccionado no existe.");

                    return;
                }
            }

            if (FechaDesde.Date > FechaHasta.Date)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "La fecha inicial no puede ser posterior a la fecha final.");

                return;
            }

            DateTime inicio = FechaDesde.Date;
            DateTime finExclusivo = FechaHasta.Date.AddDays(1);

            // Cargos registrados hasta el cierre del periodo.
            var consultaCargos = _context.CargosPaciente
                .AsNoTracking()
                .Include(c => c.Paciente)
                .Where(c => c.FechaCargo < finExclusivo);

            if (IdPaciente.HasValue)
            {
                consultaCargos = consultaCargos
                    .Where(c => c.IdPaciente == IdPaciente.Value);
            }

            var cargos = await consultaCargos
                .OrderBy(c => c.FechaCargo)
                .ToListAsync();

            // Cuotas registradas hasta el cierre del periodo.
            var consultaCuotas = _context.CuotasFamiliares
                .AsNoTracking()
                .Include(c => c.Paciente)
                .Where(c => c.FechaCuota < finExclusivo);

            if (IdPaciente.HasValue)
            {
                consultaCuotas = consultaCuotas
                    .Where(c => c.IdPaciente == IdPaciente.Value);
            }

            var cuotas = await consultaCuotas
                .OrderBy(c => c.FechaCuota)
                .ToListAsync();

            // Obtener pagos hasta la fecha final.
            // Se incluyen pagos anteriores para calcular correctamente
            // el saldo pendiente al cierre del periodo.
            var consultaPagos = _context.PagosPaciente
                .AsNoTracking()
                .Include(p => p.CargoPaciente)
                .Include(p => p.CuotaFamiliar)
                .Where(p => p.FechaPago < finExclusivo);

            if (IdPaciente.HasValue)
            {
                consultaPagos = consultaPagos.Where(p =>
                    (p.CargoPaciente != null &&
                     p.CargoPaciente.IdPaciente == IdPaciente.Value)
                    ||
                    (p.CuotaFamiliar != null &&
                     p.CuotaFamiliar.IdPaciente == IdPaciente.Value));
            }

            var pagos = await consultaPagos.ToListAsync();

            var pagosPorCargo = pagos
                .Where(p => p.IdCargo.HasValue)
                .GroupBy(p => p.IdCargo!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(p => p.MontoPagado));

            var pagosPorCuota = pagos
                .Where(p => p.IdCuota.HasValue)
                .GroupBy(p => p.IdCuota!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(p => p.MontoPagado));

            // Resumen de cargos y cuotas creados en el periodo.
            TotalCargosPeriodo = cargos
                .Where(c => c.FechaCargo >= inicio)
                .Sum(c => c.MontoFinal);

            TotalCuotasPeriodo = cuotas
                .Where(c => c.FechaCuota >= inicio)
                .Sum(c => c.Monto);

            // Pagos efectivamente registrados durante el periodo.
            TotalPagosPeriodo = pagos
                .Where(p => p.FechaPago >= inicio)
                .Sum(p => p.MontoPagado);

            // Detalle de cargos. Se muestran los del periodo y también
            // los anteriores que todavía tienen saldo pendiente.
            foreach (var cargo in cargos)
            {
                decimal pagado = pagosPorCargo.TryGetValue(
                    cargo.IdCargo, out var importePagado)
                    ? importePagado
                    : 0m;

                decimal saldo = Math.Max(
                    cargo.MontoFinal - pagado, 0m);

                SaldoPendienteAlCierre += saldo;

                bool dentroDelPeriodo =
                    cargo.FechaCargo >= inicio;

                if (!dentroDelPeriodo && saldo <= 0)
                {
                    continue;
                }

                Detalles.Add(new DetalleCobro
                {
                    IdPaciente = cargo.IdPaciente,
                    Paciente = cargo.Paciente == null
                        ? "Paciente no disponible"
                        : $"{cargo.Paciente.Nombres} {cargo.Paciente.Apellidos}",
                    Tipo = "Cargo",
                    IdReferencia = cargo.IdCargo,
                    Fecha = cargo.FechaCargo,
                    Concepto = cargo.Concepto,
                    Total = cargo.MontoFinal,
                    Pagado = pagado,
                    Saldo = saldo,
                    Estado = ObtenerEstado(cargo.MontoFinal, pagado)
                });
            }

            // Detalle de cuotas familiares.
            foreach (var cuota in cuotas)
            {
                decimal pagado = pagosPorCuota.TryGetValue(
                    cuota.IdCuota, out var importePagado)
                    ? importePagado
                    : 0m;

                decimal saldo = Math.Max(
                    cuota.Monto - pagado, 0m);

                SaldoPendienteAlCierre += saldo;

                bool dentroDelPeriodo =
                    cuota.FechaCuota >= inicio;

                if (!dentroDelPeriodo && saldo <= 0)
                {
                    continue;
                }

                Detalles.Add(new DetalleCobro
                {
                    IdPaciente = cuota.IdPaciente,
                    Paciente = cuota.Paciente == null
                        ? "Paciente no disponible"
                        : $"{cuota.Paciente.Nombres} {cuota.Paciente.Apellidos}",
                    Tipo = "Cuota",
                    IdReferencia = cuota.IdCuota,
                    Fecha = cuota.FechaCuota,
                    Concepto = "Cuota familiar",
                    Total = cuota.Monto,
                    Pagado = pagado,
                    Saldo = saldo,
                    Estado = ObtenerEstado(cuota.Monto, pagado)
                });
            }

            Detalles = Detalles
                .OrderBy(d => d.Paciente)
                .ThenByDescending(d => d.Fecha)
                .ToList();
        }

        private static string ObtenerEstado(decimal total, decimal pagado)
        {
            if (pagado <= 0)
            {
                return "Pendiente";
            }

            if (pagado >= total)
            {
                return "Pagado";
            }

            return "Abonado";
        }

        public class DetalleCobro
        {
            public int IdPaciente { get; set; }

            public string Paciente { get; set; } = "";

            public string Tipo { get; set; } = "";

            public int IdReferencia { get; set; }

            public DateTime Fecha { get; set; }

            public string Concepto { get; set; } = "";

            public decimal Total { get; set; }

            public decimal Pagado { get; set; }

            public decimal Saldo { get; set; }

            public string Estado { get; set; } = "";
        }
    }
}