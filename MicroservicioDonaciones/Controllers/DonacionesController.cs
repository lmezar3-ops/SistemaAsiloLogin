
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroservicioDonaciones.Data;

namespace MicroservicioDonaciones.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonacionesController : ControllerBase
    {
        private readonly DonacionesDbContext _context;

        public DonacionesController(DonacionesDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDonaciones()
        {
            var donaciones = await _context.Donaciones
                .Include(d => d.Donante)
                .OrderByDescending(d => d.FechaDonacion)
                .Select(d => new
                {
                    id = d.IdDonacion,
                    donante = d.Donante != null
                        ? d.Donante.Nombre
                        : "Sin donante",
                    tipo = d.TipoDonacion,
                    descripcion = d.Descripcion,
                    monto = d.Monto,
                    fecha = d.FechaDonacion.ToString("yyyy-MM-dd")
                })
                .ToListAsync();

            return Ok(donaciones);
        }
    }
}