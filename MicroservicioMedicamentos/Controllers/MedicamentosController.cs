
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroservicioMedicamentos.Data;

namespace MicroservicioMedicamentos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicamentosController : ControllerBase
    {
        private readonly MedicamentosDbContext _context;

        public MedicamentosController(MedicamentosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMedicamentos()
        {
            var medicamentos = await _context.Medicamentos
                .AsNoTracking()
                .OrderBy(m => m.Nombre)
                .Select(m => new
                {
                    idMedicamento = m.IdMedicamento,
                    nombre = m.Nombre,
                    descripcion = m.Descripcion,
                    existencia = m.Existencia,
                    fechaVencimiento = m.FechaVencimiento,
                    costo = m.Costo,
                    estado = m.Estado
                })
                .ToListAsync();

            return Ok(medicamentos);
        }

        [HttpGet("vencer")]
        public async Task<IActionResult> GetMedicamentosPorVencer()
        {
            var hoy = DateTime.Today;
            var limite = hoy.AddDays(30);

            var medicamentos = await _context.Medicamentos
                .AsNoTracking()
                .Where(m =>
                    m.FechaVencimiento.HasValue &&
                    m.FechaVencimiento.Value >= hoy &&
                    m.FechaVencimiento.Value <= limite)
                .OrderBy(m => m.FechaVencimiento)
                .Select(m => new
                {
                    id = m.IdMedicamento,
                    nombre = m.Nombre,
                    fechaVencimiento = m.FechaVencimiento
                })
                .ToListAsync();

            return Ok(medicamentos);
        }
    }
}
