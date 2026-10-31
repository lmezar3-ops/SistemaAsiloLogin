using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Controllers
{
    [ApiController]
    [Route("api/medicaciones-habituales")]
    [Authorize(Roles = "Administrador,MedicoGeneral,Medico,Enfermero")]
    public class MedicacionesHabitualesController : ControllerBase
    {
        private readonly AsiloDbContext _context;

        public MedicacionesHabitualesController(AsiloDbContext context)
        {
            _context = context;
        }

        [HttpGet("catalogo")]
        public async Task<IActionResult> ObtenerCatalogo()
        {
            var medicamentos = await _context.Database
                .SqlQuery<MedicamentoHabitualCatalogo>($"""
                    SELECT idMedicamento AS IdMedicamento,
                           nombre AS Nombre
                    FROM dbo.Medicamento
                    WHERE estado = 1
                    ORDER BY nombre
                    """)
                .ToListAsync();

            return Ok(medicamentos);
        }


        [HttpGet("{idPaciente:int}")]
        public async Task<IActionResult> ObtenerPorPaciente(int idPaciente)
        {
            if (idPaciente <= 0)
                return BadRequest(new { mensaje = "El paciente no es válido." });

            var medicaciones = await _context.MedicacionesHabituales
                .AsNoTracking()
                .Where(m => m.IdPaciente == idPaciente)
                .OrderBy(m => m.IdMedicacionHabitual)
                .Select(m => new
                {
                    idMedicacionHabitual = m.IdMedicacionHabitual,
                    idPaciente = m.IdPaciente,
                    idMedicamento = m.IdMedicamento,
                    nombreMedicamentoInformado = m.NombreMedicamentoInformado,
                    dosis = m.Dosis,
                    frecuencia = m.Frecuencia,
                    indicaciones = m.Indicaciones,
                    fechaInicio = m.FechaInicio,
                    fechaFin = m.FechaFin,
                    estado = m.Estado,
                    requiereVerificacion = m.RequiereVerificacion
                })
                .ToListAsync();

            return Ok(medicaciones);
        }

        [HttpPut("{idPaciente:int}/{idMedicacionHabitual:int}")]
        public async Task<IActionResult> Actualizar(
            int idPaciente,
            int idMedicacionHabitual,
            [FromBody] MedicacionHabitualRequest item)
        {
            if (idPaciente <= 0 || idMedicacionHabitual <= 0)
                return BadRequest(new { mensaje = "Los identificadores no son válidos." });

            var medicacion = await _context.MedicacionesHabituales
                .FirstOrDefaultAsync(m =>
                    m.IdPaciente == idPaciente &&
                    m.IdMedicacionHabitual == idMedicacionHabitual);

            if (medicacion == null)
            {
                return NotFound(new
                {
                    mensaje = "No se encontró esa medicación para el paciente indicado."
                });
            }

            bool tieneNombre =
                !string.IsNullOrWhiteSpace(item.NombreMedicamentoInformado);

            if (item.IdMedicamento.HasValue == tieneNombre)
            {
                return BadRequest(new
                {
                    mensaje = "Seleccione un medicamento del catálogo o escriba un nombre informado, pero no ambos."
                });
            }

            if (item.NombreMedicamentoInformado?.Trim().Length > 150 ||
                item.Dosis?.Length > 100 ||
                item.Frecuencia?.Length > 100 ||
                item.Indicaciones?.Length > 500)
            {
                return BadRequest(new
                {
                    mensaje = "Uno de los campos supera la longitud permitida."
                });
            }

            if (item.FechaInicio.HasValue &&
                item.FechaFin.HasValue &&
                item.FechaFin.Value.Date < item.FechaInicio.Value.Date)
            {
                return BadRequest(new
                {
                    mensaje = "La fecha de finalización no puede ser anterior a la fecha de inicio."
                });
            }

            if (item.IdMedicamento.HasValue)
            {
                var existe = await _context.Database
                    .SqlQuery<int>($"""
                        SELECT COUNT(*) AS [Value]
                        FROM dbo.Medicamento
                        WHERE idMedicamento = {item.IdMedicamento.Value}
                        """)
                    .SingleAsync();

                if (existe == 0)
                {
                    return BadRequest(new
                    {
                        mensaje = "El medicamento seleccionado no existe en el catálogo."
                    });
                }
            }

            bool fueraCatalogo = !item.IdMedicamento.HasValue;

            medicacion.IdMedicamento = item.IdMedicamento;

            medicacion.NombreMedicamentoInformado = fueraCatalogo
                ? item.NombreMedicamentoInformado!.Trim()
                : null;

            medicacion.Dosis = item.Dosis?.Trim();
            medicacion.Frecuencia = item.Frecuencia?.Trim();
            medicacion.Indicaciones = item.Indicaciones?.Trim();
            medicacion.FechaInicio = item.FechaInicio?.Date;
            medicacion.FechaFin = item.FechaFin?.Date;
            medicacion.Estado = item.Estado;
            medicacion.RequiereVerificacion = fueraCatalogo;

            try
            {
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Medicación habitual actualizada correctamente."
                });
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    mensaje = "No se pudo actualizar la medicación."
                });
            }
        }


        [HttpPost("{idPaciente:int}")]
        public async Task<IActionResult> Guardar(
            int idPaciente,
            [FromBody] List<MedicacionHabitualRequest> medicaciones)
        {
            if (idPaciente <= 0)
                return BadRequest(new { mensaje = "El paciente no es válido." });

            if (medicaciones == null || medicaciones.Count == 0)
                return BadRequest(new { mensaje = "No se recibieron medicaciones." });

            if (!await _context.Pacientes.AnyAsync(p => p.IdPaciente == idPaciente))
                return NotFound(new { mensaje = "No se encontró el paciente." });

            foreach (var item in medicaciones)
            {
                bool tieneNombre =
                    !string.IsNullOrWhiteSpace(item.NombreMedicamentoInformado);

                if (item.IdMedicamento.HasValue == tieneNombre)
                {
                    return BadRequest(new
                    {
                        mensaje = "Cada medicación debe seleccionar un medicamento del catálogo o indicar un nombre fuera del catálogo, pero no ambos."
                    });
                }

                if (item.NombreMedicamentoInformado?.Trim().Length > 150)
                    return BadRequest(new { mensaje = "El nombre informado supera los 150 caracteres." });

                if (item.Dosis?.Length > 100 ||
                    item.Frecuencia?.Length > 100 ||
                    item.Indicaciones?.Length > 500)
                {
                    return BadRequest(new
                    {
                        mensaje = "La dosis, frecuencia o indicaciones superan la longitud permitida."
                    });
                }

                if (item.FechaInicio.HasValue &&
                    item.FechaFin.HasValue &&
                    item.FechaFin.Value.Date < item.FechaInicio.Value.Date)
                {
                    return BadRequest(new
                    {
                        mensaje = "La fecha de finalización no puede ser anterior a la fecha de inicio."
                    });
                }

                if (item.IdMedicamento.HasValue)
                {
                    var existeMedicamento = await _context.Database
                        .SqlQuery<int>($"""
                            SELECT COUNT(*) AS [Value]
                            FROM dbo.Medicamento
                            WHERE idMedicamento = {item.IdMedicamento.Value}
                            """)
                        .SingleAsync();

                    if (existeMedicamento == 0)
                    {
                        return BadRequest(new
                        {
                            mensaje = "Uno de los medicamentos seleccionados no existe en el catálogo."
                        });
                    }
                }
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in medicaciones)
                {
                    bool fueraCatalogo =
                        !item.IdMedicamento.HasValue;

                    _context.MedicacionesHabituales.Add(
                        new MedicacionHabitual
                        {
                            IdPaciente = idPaciente,
                            IdMedicamento = item.IdMedicamento,
                            NombreMedicamentoInformado =
                                fueraCatalogo
                                    ? item.NombreMedicamentoInformado!.Trim()
                                    : null,
                            Dosis = item.Dosis?.Trim(),
                            Frecuencia = item.Frecuencia?.Trim(),
                            Indicaciones = item.Indicaciones?.Trim(),
                            FechaInicio = item.FechaInicio?.Date,
                            FechaFin = item.FechaFin?.Date,
                            Estado = item.Estado,
                            RequiereVerificacion = fueraCatalogo
                        });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje = "Medicación habitual guardada correctamente.",
                    cantidad = medicaciones.Count
                });
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();

                return Conflict(new
                {
                    mensaje = "No se pudieron guardar las medicaciones. Verifique los datos y las relaciones de la base de datos."
                });
            }
        }
    }

    public class MedicamentoHabitualCatalogo
    {
        public int IdMedicamento { get; set; }
        public string Nombre { get; set; } = "";
    }
    public class MedicacionHabitualRequest
    {
        public int? IdMedicamento { get; set; }

        public string? NombreMedicamentoInformado { get; set; }

        public string? Dosis { get; set; }

        public string? Frecuencia { get; set; }

        public string? Indicaciones { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public bool Estado { get; set; } = true;
    }
}

