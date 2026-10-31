
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador,CajaFinanzas")]
    public class CajaPagosModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
