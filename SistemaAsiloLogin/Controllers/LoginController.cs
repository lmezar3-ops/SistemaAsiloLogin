using Microsoft.AspNetCore.Mvc;
using SistemaAsiloLogin.Servicios;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;


namespace SistemaAsiloLogin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly AutenticacionService _autenticacionService;

        public LoginController(AutenticacionService autenticacionService)
        {
            _autenticacionService = autenticacionService;
        }

        [HttpPost]
        public async Task<IActionResult> IniciarSesion([FromBody] LoginRequest request)
        {
            var usuario = await _autenticacionService.IniciarSesion(
                request.NombreUsuario,
                request.Contraseña);

            if (usuario == null)
            {
                return Unauthorized(new
                {
                    mensaje = "Usuario o contraseña incorrectos"
                });
            }
//
            var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
    new Claim(ClaimTypes.Role, usuario.Rol)
};

            var identidad = new ClaimsIdentity(
                claims,
                Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

            var propiedades = new AuthenticationProperties
            {
                IsPersistent = false
            };

            await HttpContext.SignInAsync(
                Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad),
                propiedades);
//
            return Ok(new
            {
                mensaje = "Inicio de sesión correcto",
                usuario = usuario.NombreUsuario,
                rol = usuario.Rol
            });
        }
    }

    public class LoginRequest
    {
        public string NombreUsuario { get; set; } = "";
        public string Contraseña { get; set; } = "";
    }
}
