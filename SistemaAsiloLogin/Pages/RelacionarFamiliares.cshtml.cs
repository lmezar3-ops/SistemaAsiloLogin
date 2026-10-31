using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador")]
    public class RelacionarFamiliaresModel : PageModel
    {
        private readonly AsiloDbContext _context;

        public RelacionarFamiliaresModel(AsiloDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int IdPaciente { get; set; }

        [BindProperty]
        public int IdFamiliar { get; set; }

        [BindProperty]
        public string Parentesco { get; set; } = "";

        [BindProperty]
        public bool EsResponsable { get; set; }

        public List<SelectListItem> PacientesDisponibles { get; set; }
            = new();

        public List<SelectListItem> FamiliaresDisponibles { get; set; }
            = new();

        public List<PacienteFamiliar> Relaciones { get; set; }
            = new();

        public async Task OnGetAsync()
        {
            await CargarDatosAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Parentesco = (Parentesco ?? "").Trim();

            if (IdPaciente <= 0)
                ModelState.AddModelError(nameof(IdPaciente),
                    "Selecciona un paciente.");

            if (IdFamiliar <= 0)
                ModelState.AddModelError(nameof(IdFamiliar),
                    "Selecciona un familiar.");

            if (string.IsNullOrWhiteSpace(Parentesco))
                ModelState.AddModelError(nameof(Parentesco),
                    "El parentesco con el paciente es obligatorio.");

            if (!ModelState.IsValid)
            {
                await CargarDatosAsync();
                return Page();
            }

            try
            {
                var pacienteExiste = await _context.Pacientes
                    .AnyAsync(p => p.IdPaciente == IdPaciente);

                if (!pacienteExiste)
                {
                    ModelState.AddModelError(nameof(IdPaciente),
                        "El paciente seleccionado no existe.");

                    await CargarDatosAsync();
                    return Page();
                }

                var familiar = await _context.Familiares
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.IdFamiliar == IdFamiliar);

                if (familiar == null || !familiar.Estado)
                {
                    ModelState.AddModelError(nameof(IdFamiliar),
                        "El familiar no existe o está inactivo.");

                    await CargarDatosAsync();
                    return Page();
                }

                var relacionDuplicada = await _context.PacientesFamiliares
                    .AnyAsync(pf =>
                        pf.IdPaciente == IdPaciente &&
                        pf.IdFamiliar == IdFamiliar);

                if (relacionDuplicada)
                {
                    TempData["Error"] =
                        "Este familiar ya está relacionado con ese paciente.";

                    await CargarDatosAsync();
                    return Page();
                }

                var relacionesPaciente = await _context.PacientesFamiliares
                    .Where(pf => pf.IdPaciente == IdPaciente)
                    .ToListAsync();

                var tieneResponsable = relacionesPaciente
                    .Any(pf => pf.EsResponsable);

                // Toda relación nueva debe respetar la existencia
                // de un responsable para el paciente.
                if (!tieneResponsable && !EsResponsable)
                {
                    ModelState.AddModelError(nameof(EsResponsable),
                        "La primera relación debe designarse como responsable.");

                    await CargarDatosAsync();
                    return Page();
                }

                // Si se designa un nuevo responsable, se desmarca
                // el responsable anterior del mismo paciente.
                if (EsResponsable)
                {
                    foreach (var relacion in relacionesPaciente)
                    {
                        relacion.EsResponsable = false;
                    }
                }

                var nuevaRelacion = new PacienteFamiliar
                {
                    IdPaciente = IdPaciente,
                    IdFamiliar = IdFamiliar,
                    Parentesco = Parentesco,
                    EsResponsable = EsResponsable
                };

                _context.PacientesFamiliares.Add(nuevaRelacion);

                await _context.SaveChangesAsync();

                TempData["Mensaje"] =
                    "La relación entre el paciente y el familiar se registró correctamente.";

                return RedirectToPage();
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "No se pudo guardar la relación. Verifica los datos e inténtalo nuevamente.";

                await CargarDatosAsync();
                return Page();
            }
        }

        private async Task CargarDatosAsync()
        {
            PacientesDisponibles = await _context.Pacientes
                .AsNoTracking()
                .OrderBy(p => p.Apellidos)
                .ThenBy(p => p.Nombres)
                .Select(p => new SelectListItem
                {
                    Value = p.IdPaciente.ToString(),
                    Text = p.Nombres + " " + p.Apellidos
                })
                .ToListAsync();

            FamiliaresDisponibles = await _context.Familiares
                .AsNoTracking()
                .Where(f => f.Estado)
                .OrderBy(f => f.Apellidos)
                .ThenBy(f => f.Nombres)
                .Select(f => new SelectListItem
                {
                    Value = f.IdFamiliar.ToString(),
                    Text = f.Nombres + " " + f.Apellidos
                })
                .ToListAsync();

            Relaciones = await _context.PacientesFamiliares
                .AsNoTracking()
                .Include(pf => pf.Paciente)
                .Include(pf => pf.Familiar)
                .OrderBy(pf => pf.Paciente!.Apellidos)
                .ThenBy(pf => pf.Paciente!.Nombres)
                .ToListAsync();
        }
    }
}

