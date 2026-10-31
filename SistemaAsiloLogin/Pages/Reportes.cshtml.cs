using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador")]
    public class ReportesModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public ReportesModel(AsiloDbContext context)
        {
            _context = context;
        }

        public int TotalPacientes { get; private set; }
        public int TotalFamiliares { get; private set; }
        public int TotalMedicos { get; private set; }
        public int TotalEnfermeros { get; private set; }
        public int TotalCitas { get; private set; }
        public int TotalSolicitudes { get; private set; }
        public int TotalVisitas { get; private set; }
        public int TotalExamenes { get; private set; }
        public int TotalResultados { get; private set; }
        public int TotalTratamientos { get; private set; }
        public int TotalDonaciones { get; private set; }
        public int TotalDonantes { get; private set; }
        public int TotalCargos { get; private set; }
        public int TotalPagosPaciente { get; private set; }
        public int TotalCuotas { get; private set; }
        public int TotalPagosFundacion { get; private set; }
        public int TotalGastos { get; private set; }
        public int TotalMovimientosCaja { get; private set; }
        public decimal TotalMontoDonaciones { get; private set; }
        public decimal TotalPagosRecibidos { get; private set; }
        public decimal TotalPagosFundacionMonto { get; private set; }
        public decimal TotalMontoGastos { get; private set; }
        public decimal TotalIngresosCaja { get; private set; }
        public decimal TotalEgresosCaja { get; private set; }
        public decimal SaldoCaja { get; private set; }

        public async Task OnGetAsync()
        {
            TotalPacientes = await _context.Pacientes.CountAsync();
            TotalFamiliares = await _context.Familiares.CountAsync();
            TotalMedicos = await _context.Medicos.CountAsync();
            TotalEnfermeros = await _context.Enfermeros.CountAsync();
            TotalCitas = await _context.CitasMedicas.CountAsync();
            TotalSolicitudes = await _context.SolicitudesMedicas.CountAsync();
            TotalVisitas = await _context.VisitasMedicas.CountAsync();
            TotalExamenes = await _context.ExamenesLaboratorio.CountAsync();
            TotalResultados = await _context.ResultadosExamen.CountAsync();
            TotalTratamientos = await _context.Tratamientos.CountAsync();
            TotalDonaciones = await _context.Donaciones.CountAsync();
            TotalDonantes = await _context.Donantes.CountAsync();
            TotalCargos = await _context.CargosPaciente.CountAsync();
            TotalPagosPaciente = await _context.PagosPaciente.CountAsync();
            TotalCuotas = await _context.CuotasFamiliares.CountAsync();
            TotalPagosFundacion = await _context.PagosFundacion.CountAsync();
            TotalGastos = await _context.Gastos.CountAsync();
            TotalMovimientosCaja = await _context.MovimientosCaja.CountAsync();

            TotalMontoDonaciones = await _context.Donaciones
    .SumAsync(x => (decimal?)x.Monto) ?? 0m;

            TotalPagosRecibidos = await _context.PagosPaciente
                .SumAsync(x => (decimal?)x.MontoPagado) ?? 0m;

            TotalPagosFundacionMonto = await _context.PagosFundacion
                .Where(x => x.Estado == "Pagado")
                .SumAsync(x => (decimal?)x.Monto) ?? 0m;

            TotalMontoGastos = await _context.Gastos
                .SumAsync(x => (decimal?)x.Monto) ?? 0m;

            TotalIngresosCaja = await _context.MovimientosCaja
                 .Where(m => m.TipoMovimiento == "Ingreso"
        && m.Estado == "Registrado")
                 .SumAsync(m => (decimal?)m.Monto) ?? 0m;

            TotalEgresosCaja = await _context.MovimientosCaja
                .Where(m => m.TipoMovimiento == "Egreso"
                    && m.Estado == "Registrado")
                .SumAsync(m => (decimal?)m.Monto) ?? 0m;

            SaldoCaja = TotalIngresosCaja - TotalEgresosCaja;
        }
    }
}
