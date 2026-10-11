using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    public class EstadoCuentaPacienteModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public EstadoCuentaPacienteModel(AsiloDbContext context)
        {
            _context = context;
        }

        public List<Paciente> Pacientes { get; set; } = new();

        public Paciente? PacienteSeleccionado { get; set; }

        public List<DetalleCuenta> Cargos { get; set; } = new();

        public List<DetalleCuenta> Cuotas { get; set; } = new();

        public List<DetallePago> HistorialPagos { get; set; } = new();

        public decimal TotalAdeudado =>
            Cargos.Sum(c => c.Total) + Cuotas.Sum(c => c.Total);

        public decimal TotalPagado =>
            Cargos.Sum(c => c.Pagado) + Cuotas.Sum(c => c.Pagado);

        public decimal SaldoPendiente =>
            Cargos.Sum(c => c.Saldo) + Cuotas.Sum(c => c.Saldo);

        [BindProperty(SupportsGet = true)]
        public int? IdPaciente { get; set; }

        public async Task OnGetAsync()
        {
            Pacientes = await _context.Pacientes
                .OrderBy(p => p.Apellidos)
                .ThenBy(p => p.Nombres)
                .ToListAsync();

            if (!IdPaciente.HasValue)
            {
                return;
            }

            PacienteSeleccionado = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.IdPaciente == IdPaciente.Value);

            if (PacienteSeleccionado == null)
            {
                return;
            }

            var cargos = await _context.CargosPaciente
                .Where(c => c.IdPaciente == IdPaciente.Value)
                .OrderByDescending(c => c.FechaCargo)
                .ToListAsync();

            var cuotas = await _context.CuotasFamiliares
                .Where(c => c.IdPaciente == IdPaciente.Value)
                .OrderByDescending(c => c.FechaCuota)
                .ToListAsync();

            var pagos = await _context.PagosPaciente
                .Include(p => p.CargoPaciente)
                .Include(p => p.CuotaFamiliar)
                .Where(p =>
                    (p.CargoPaciente != null &&
                     p.CargoPaciente.IdPaciente == IdPaciente.Value)
                    ||
                    (p.CuotaFamiliar != null &&
                     p.CuotaFamiliar.IdPaciente == IdPaciente.Value)
                )
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            foreach (var cargo in cargos)
            {
                decimal pagado = pagos
                    .Where(p => p.IdCargo == cargo.IdCargo)
                    .Sum(p => p.MontoPagado);

                decimal saldo = Math.Max(cargo.MontoFinal - pagado, 0);

                Cargos.Add(new DetalleCuenta
                {
                    Id = cargo.IdCargo,
                    Fecha = cargo.FechaCargo,
                    Concepto = cargo.Concepto,
                    Total = cargo.MontoFinal,
                    Pagado = pagado,
                    Saldo = saldo,
                    Estado = ObtenerEstado(cargo.MontoFinal, pagado)
                });
            }

            foreach (var cuota in cuotas)
            {
                decimal pagado = pagos
                    .Where(p => p.IdCuota == cuota.IdCuota)
                    .Sum(p => p.MontoPagado);

                decimal saldo = Math.Max(cuota.Monto - pagado, 0);

                Cuotas.Add(new DetalleCuenta
                {
                    Id = cuota.IdCuota,
                    Fecha = cuota.FechaCuota,
                    Concepto = "Cuota familiar",
                    Total = cuota.Monto,
                    Pagado = pagado,
                    Saldo = saldo,
                    Estado = ObtenerEstado(cuota.Monto, pagado)
                });
            }

            foreach (var pago in pagos)
            {
                string referencia;

                if (pago.IdCargo.HasValue)
                {
                    referencia = $"Cargo #{pago.IdCargo}";
                }
                else if (pago.IdCuota.HasValue)
                {
                    referencia = $"Cuota #{pago.IdCuota}";
                }
                else
                {
                    referencia = "Sin referencia";
                }

                HistorialPagos.Add(new DetallePago
                {
                    IdPago = pago.IdPago,
                    FechaPago = pago.FechaPago,
                    Referencia = referencia,
                    Monto = pago.MontoPagado,
                    MetodoPago = pago.MetodoPago,
                    Observaciones = pago.Observaciones
                });
            }
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

        public class DetalleCuenta
        {
            public int Id { get; set; }

            public DateTime Fecha { get; set; }

            public string Concepto { get; set; } = "";

            public decimal Total { get; set; }

            public decimal Pagado { get; set; }

            public decimal Saldo { get; set; }

            public string Estado { get; set; } = "";
        }

        public class DetallePago
        {
            public int IdPago { get; set; }

            public DateTime FechaPago { get; set; }

            public string Referencia { get; set; } = "";

            public decimal Monto { get; set; }

            public string MetodoPago { get; set; } = "";

            public string Observaciones { get; set; } = "";
        }
    }
}
