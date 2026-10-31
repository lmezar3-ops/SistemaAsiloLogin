using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador,MedicoGeneral,Medico,Enfermero,Farmacia")]
    public class MedicamentosModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
