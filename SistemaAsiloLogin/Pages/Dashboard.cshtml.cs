using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace SistemaAsiloLogin.Pages
{

    [Authorize]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
        }
    }
}
