using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace SistemaAsiloLogin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogoutController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync();

            return Ok(new
            {
                mensaje = "Sesión cerrada correctamente"
            });
        }
    }
}
