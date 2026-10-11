using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroservicioAncianos.Data;
using MicroservicioAncianos.Models;

namespace MicroservicioAncianos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AncianosController : ControllerBase
    {
        private readonly AsiloDbContext _context;


    public AncianosController(AsiloDbContext context)
        {
            _context = context;
        }

        // GET: api/Ancianos
        [HttpGet]
        public async Task<IActionResult> GetAncianos()
        {
            var pacientes = await _context.Pacientes
                .AsNoTracking()
                .ToListAsync();

            return Ok(pacientes);
        }

        // POST: api/Ancianos
        [HttpPost]
        public async Task<IActionResult> RegistrarAnciano(
            [FromBody] Paciente nuevoPaciente)
        {
            // Normalizar el DPI recibido.
            string dpi = nuevoPaciente.DPI?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(dpi))
            {
                return BadRequest(new
                {
                    mensaje = "El DPI es obligatorio."
                });
            }

            if (dpi.Length != 13 || !dpi.All(char.IsAsciiDigit))
            {
                return BadRequest(new
                {
                    mensaje = "El DPI debe contener exactamente 13 dígitos numéricos."
                });
            }

            // Comprobar si el DPI ya pertenece a otro registro.
            bool existeDpi = await _context.Pacientes
                .AnyAsync(p => p.DPI == dpi);

            if (existeDpi)
            {
                return Conflict(new
                {
                    mensaje = "Ya existe un paciente registrado con ese DPI."
                });
            }

            nuevoPaciente.DPI = dpi;

            _context.Pacientes.Add(nuevoPaciente);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // El índice único de SQL Server protege también
                // frente a intentos simultáneos de registro.
                return Conflict(new
                {
                    mensaje = "No se pudo registrar el paciente. Verifique que el DPI no esté registrado y que los datos sean válidos."
                });
            }

            return Ok(new
            {
                mensaje = "Paciente registrado correctamente.",
                paciente = nuevoPaciente
            });
        }
    }

}
