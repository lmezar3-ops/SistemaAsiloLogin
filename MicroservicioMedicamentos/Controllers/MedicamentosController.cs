using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroservicioMedicamentos.Data;
using MicroservicioMedicamentos.Models;

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

        // Consultar el inventario completo.
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

        // Consultar medicamentos que vencen en los próximos 30 días.
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
        // Registrar un medicamento nuevo.
        [HttpPost]
        public async Task<IActionResult> CrearMedicamento(
            [FromBody] Medicamento medicamento)
        {
            if (string.IsNullOrWhiteSpace(medicamento.Nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre del medicamento es obligatorio."
                });
            }

            if (medicamento.Nombre.Trim().Length > 150)
            {
                return BadRequest(new
                {
                    mensaje = "El nombre no puede superar los 150 caracteres."
                });
            }

            if (medicamento.Descripcion?.Length > 300)
            {
                return BadRequest(new
                {
                    mensaje = "La descripción no puede superar los 300 caracteres."
                });
            }

            if (medicamento.Existencia < 0)
            {
                return BadRequest(new
                {
                    mensaje = "La existencia no puede ser negativa."
                });
            }

            if (medicamento.Costo < 0 || medicamento.Costo > 99999999.99m)
            {
                return BadRequest(new
                {
                    mensaje = "El costo debe ser un valor válido y no negativo."
                });
            }

            medicamento.IdMedicamento = 0;
            medicamento.Nombre = medicamento.Nombre.Trim();
            medicamento.Descripcion =
                string.IsNullOrWhiteSpace(medicamento.Descripcion)
                    ? null
                    : medicamento.Descripcion.Trim();

            _context.Medicamentos.Add(medicamento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMedicamentos),
                new { id = medicamento.IdMedicamento },
                new
                {
                    mensaje = "Medicamento registrado correctamente.",
                    idMedicamento = medicamento.IdMedicamento,
                    nombre = medicamento.Nombre
                });
        }

        // Registrar la entrega de un medicamento prescrito.
        [HttpPost("entregas")]
        public async Task<IActionResult> RegistrarEntrega(
            [FromBody] SolicitudEntregaMedicamento solicitud)
        {
            if (solicitud.IdTratamiento <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El identificador del tratamiento no es válido."
                });
            }

            await using var transaccion =
                await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable);

            try
            {
                // Comprobar si este tratamiento ya fue entregado.
                var entregaExistente = await _context.EntregasMedicamento
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e =>
                        e.IdTratamiento == solicitud.IdTratamiento);

                if (entregaExistente != null)
                {
                    await transaccion.RollbackAsync();

                    return Conflict(new
                    {
                        mensaje = "Este tratamiento ya tiene una entrega registrada.",
                        idEntrega = entregaExistente.IdEntrega,
                        idTratamiento = entregaExistente.IdTratamiento,
                        idMedicamento = entregaExistente.IdMedicamento,
                        cantidad = entregaExistente.Cantidad,
                        fechaEntrega = entregaExistente.FechaEntrega
                    });
                }

                // Obtener el medicamento y la cantidad desde el tratamiento.
                var tratamiento = await _context.Database
                    .SqlQuery<TratamientoEntregaConsulta>($"""
                SELECT
                    idTratamiento AS IdTratamiento,
                    idMedicamento AS IdMedicamento,
                    cantidad AS Cantidad
                FROM dbo.Tratamiento
                WHERE idTratamiento = {solicitud.IdTratamiento}
                """)
                    .FirstOrDefaultAsync();

                if (tratamiento == null)
                {
                    await transaccion.RollbackAsync();

                    return NotFound(new
                    {
                        mensaje = "No existe el tratamiento indicado."
                    });
                }

                if (tratamiento.IdMedicamento <= 0 ||
                    tratamiento.Cantidad <= 0)
                {
                    await transaccion.RollbackAsync();

                    return BadRequest(new
                    {
                        mensaje = "El tratamiento tiene un medicamento o una cantidad no válidos."
                    });
                }

                // Descontar solamente si el medicamento está activo
                // y tiene suficientes existencias.
                var filasActualizadas = await _context.Database
                    .ExecuteSqlInterpolatedAsync($"""
                UPDATE dbo.Medicamento
                SET existencia = existencia - {tratamiento.Cantidad}
                WHERE idMedicamento = {tratamiento.IdMedicamento}
                  AND estado = 1
                  AND existencia >= {tratamiento.Cantidad}
                """);

                if (filasActualizadas != 1)
                {
                    await transaccion.RollbackAsync();

                    return Conflict(new
                    {
                        mensaje = "El medicamento está inactivo o no tiene existencias suficientes."
                    });
                }

                // Registrar la entrega dentro de la misma transacción.
                var entrega = new EntregaMedicamento
                {
                    IdTratamiento = tratamiento.IdTratamiento,
                    IdMedicamento = tratamiento.IdMedicamento,
                    Cantidad = tratamiento.Cantidad
                };

                _context.EntregasMedicamento.Add(entrega);

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                return Ok(new
                {
                    mensaje = "Entrega registrada correctamente.",
                    idEntrega = entrega.IdEntrega,
                    idTratamiento = entrega.IdTratamiento,
                    idMedicamento = entrega.IdMedicamento,
                    cantidad = entrega.Cantidad,
                    fechaEntrega = entrega.FechaEntrega
                });
            }
            catch (DbUpdateException)
            {
                await transaccion.RollbackAsync();

                return Conflict(new
                {
                    mensaje = "No se pudo registrar la entrega. Comprueba si ya existe una entrega para este tratamiento."
                });
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }
    }
}
