using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador")]
    public class FamiliaresModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public FamiliaresModel(AsiloDbContext context)
        {
            _context = context;
        }

        public List<Familiar> ListaFamiliares { get; set; } = new();

        [BindProperty]
        public Familiar NuevoFamiliar { get; set; } = new();

        [BindProperty]
        public Familiar Edicion { get; set; } = new();

        public bool ModoEdicion { get; set; }

        public async Task OnGetAsync(int? editar)
        {
            await CargarFamiliaresAsync();
            NuevoFamiliar.Estado = true;

            if (editar.HasValue)
            {
                var familiar = await _context.Familiares
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.IdFamiliar == editar.Value);

                if (familiar != null)
                {
                    Edicion = familiar;
                    ModoEdicion = true;
                }
                else
                {
                    TempData["Error"] = "No se encontró el familiar.";
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
           
            ModelState.Remove("Nombres");
            ModelState.Remove("Apellidos");
            ModelState.Remove("Parentesco");
            //          
            if (!ModelState.IsValid)
            {

                var errores = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e =>
                        $"{x.Key}: {e.ErrorMessage}"))
                    .ToList();

                TempData["Error"] = "Errores de validación: " +
                    string.Join(" | ", errores);

                await CargarFamiliaresAsync();
                return Page();
            }

            try
            {
                NuevoFamiliar.Nombres = NuevoFamiliar.Nombres.Trim();
                NuevoFamiliar.Apellidos = NuevoFamiliar.Apellidos.Trim();
                NuevoFamiliar.Parentesco = NuevoFamiliar.Parentesco.Trim();
                NuevoFamiliar.Telefono = NuevoFamiliar.Telefono?.Trim() ?? "";
                NuevoFamiliar.CorreoElectronico =
                    NuevoFamiliar.CorreoElectronico?.Trim() ?? "";
                NuevoFamiliar.Direccion =
                    NuevoFamiliar.Direccion?.Trim() ?? "";

                _context.Familiares.Add(NuevoFamiliar);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Familiar registrado correctamente.";
                return RedirectToPage();
            }
            catch (Exception)
            {
                TempData["Error"] = "No se pudo registrar el familiar.";
                await CargarFamiliaresAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostActualizarAsync()
        {
            ModelState.Clear();

            if (Edicion.IdFamiliar <= 0)
            {
                TempData["Error"] = "El identificador del familiar no es válido.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(Edicion.Nombres) ||
                string.IsNullOrWhiteSpace(Edicion.Apellidos) ||
                string.IsNullOrWhiteSpace(Edicion.Parentesco))
            {
                TempData["Error"] =
                    "Nombres, apellidos y parentesco son obligatorios.";

                await CargarFamiliaresAsync();
                ModoEdicion = true;
                return Page();
            }

            try
            {
                var familiar = await _context.Familiares
                    .FirstOrDefaultAsync(f =>
                        f.IdFamiliar == Edicion.IdFamiliar);

                if (familiar == null)
                {
                    TempData["Error"] = "No se encontró el familiar.";
                    return RedirectToPage();
                }

                familiar.Nombres = Edicion.Nombres.Trim();
                familiar.Apellidos = Edicion.Apellidos.Trim();
                familiar.Parentesco = Edicion.Parentesco.Trim();
                familiar.Telefono = Edicion.Telefono?.Trim() ?? "";
                familiar.CorreoElectronico =
                    Edicion.CorreoElectronico?.Trim() ?? "";
                familiar.Direccion = Edicion.Direccion?.Trim() ?? "";
                familiar.Estado = Edicion.Estado;

                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Familiar actualizado correctamente.";
                return RedirectToPage();
            }
            catch (Exception)
            {
                TempData["Error"] = "No se pudieron guardar los cambios.";
                await CargarFamiliaresAsync();
                ModoEdicion = true;
                return Page();
            }
        }

        private async Task CargarFamiliaresAsync()
        {
            ListaFamiliares = await _context.Familiares
                .AsNoTracking()
                .OrderBy(f => f.Apellidos)
                .ThenBy(f => f.Nombres)
                .ToListAsync();
        }
    }
}