using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador")]
    public class AdministracionUsuariosModel : PageModel
    {
        private readonly AsiloDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public AdministracionUsuariosModel(AsiloDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        public List<Usuario> Usuarios { get; set; } = new();
        public List<Especialidad> Especialidades { get; set; } = new();

        [BindProperty]
        public UsuarioFormulario NuevoUsuario { get; set; } = new();

        public class UsuarioFormulario
        {
            public string NombreCompleto { get; set; } = "";

            public string NombresMedico { get; set; } = "";

            public string ApellidosMedico { get; set; } = "";

            [Required(ErrorMessage = "Seleccione la fecha de nacimiento.")]
            [DataType(DataType.Date)]
            public DateTime? FechaNacimiento { get; set; }

            [Required(ErrorMessage = "Ingrese el área.")]
            [StringLength(100)]
            public string Area { get; set; } = "";

            [Required(ErrorMessage = "Ingrese el puesto.")]
            [StringLength(100)]
            public string Puesto { get; set; } = "";

            [Required(ErrorMessage = "Ingrese el correo electrónico.")]
            [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
            [StringLength(150)]
            public string CorreoElectronico { get; set; } = "";

            [StringLength(20)]
            public string Telefono { get; set; } = "";

            [Required(ErrorMessage = "Ingrese el nombre de usuario.")]
            [StringLength(50, MinimumLength = 3,
                ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres.")]
            [RegularExpression(@"^[a-zA-Z0-9._-]+$",
                ErrorMessage = "Use letras, números, punto, guion o guion bajo.")]
            public string NombreUsuario { get; set; } = "";

            [Required(ErrorMessage = "Ingrese una contraseña.")]
            [StringLength(100, MinimumLength = 12,
                ErrorMessage = "La contraseña debe tener al menos 12 caracteres.")]
            [DataType(DataType.Password)]
            public string Contraseña { get; set; } = "";

            [Required(ErrorMessage = "Seleccione un rol.")]
            public string Rol { get; set; } = "";

            public int? IdEspecialidad { get; set; }
        }

        private static readonly string[] RolesPermitidos =
        {
            "Administrador",
            "MedicoGeneral",
            "Medico",
            "Enfermero",
            "Fundacion",
            "Laboratorio",
            "Farmacia",
            "CajaFinanzas",
            "Donaciones"
        };

        public async Task OnGetAsync()
        {
            await CargarDatosAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            NuevoUsuario.NombreCompleto ??= "";
            NuevoUsuario.NombresMedico ??= "";
            NuevoUsuario.ApellidosMedico ??= "";
            NuevoUsuario.Area ??= "";
            NuevoUsuario.Puesto ??= "";
            NuevoUsuario.CorreoElectronico ??= "";
            NuevoUsuario.NombreUsuario ??= "";
            NuevoUsuario.Contraseña ??= "";
            NuevoUsuario.Rol ??= "";
            NuevoUsuario.Telefono ??= "";

            bool esMedico = NuevoUsuario.Rol == "Medico";
            bool esMedicoGeneral = NuevoUsuario.Rol == "MedicoGeneral";
            bool requierePerfilMedico = esMedico || esMedicoGeneral;

            if (!RolesPermitidos.Contains(NuevoUsuario.Rol))
            {
                ModelState.AddModelError(
                    "NuevoUsuario.Rol",
                    "El rol seleccionado no es válido.");
            }

            if (NuevoUsuario.FechaNacimiento.HasValue &&
                NuevoUsuario.FechaNacimiento.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    "NuevoUsuario.FechaNacimiento",
                    "La fecha de nacimiento no puede ser futura.");
            }

            if (requierePerfilMedico)
            {
                if (string.IsNullOrWhiteSpace(NuevoUsuario.NombresMedico))
                {
                    ModelState.AddModelError(
                        "NuevoUsuario.NombresMedico",
                        "Ingrese los nombres del médico.");
                }

                if (string.IsNullOrWhiteSpace(NuevoUsuario.ApellidosMedico))
                {
                    ModelState.AddModelError(
                        "NuevoUsuario.ApellidosMedico",
                        "Ingrese los apellidos del médico.");
                }

                if (NuevoUsuario.NombresMedico.Trim().Length > 100)
                {
                    ModelState.AddModelError(
                        "NuevoUsuario.NombresMedico",
                        "Los nombres no pueden superar 100 caracteres.");
                }

                if (NuevoUsuario.ApellidosMedico.Trim().Length > 100)
                {
                    ModelState.AddModelError(
                        "NuevoUsuario.ApellidosMedico",
                        "Los apellidos no pueden superar 100 caracteres.");
                }
            }
            else if (string.IsNullOrWhiteSpace(NuevoUsuario.NombreCompleto))
            {
                ModelState.AddModelError(
                    "NuevoUsuario.NombreCompleto",
                    "Ingrese el nombre completo.");
            }

            if (esMedico && !NuevoUsuario.IdEspecialidad.HasValue)
            {
                ModelState.AddModelError(
                    "NuevoUsuario.IdEspecialidad",
                    "Seleccione una especialidad para el médico especialista.");
            }

            if (esMedicoGeneral && NuevoUsuario.IdEspecialidad.HasValue)
            {
                ModelState.AddModelError(
                    "NuevoUsuario.IdEspecialidad",
                    "El médico general no debe tener una especialidad asignada.");
            }

            if (NuevoUsuario.IdEspecialidad.HasValue)
            {
                bool existeEspecialidad = await _context.Especialidades
                    .AnyAsync(e => e.IdEspecialidad == NuevoUsuario.IdEspecialidad.Value);

                if (!existeEspecialidad)
                {
                    ModelState.AddModelError(
                        "NuevoUsuario.IdEspecialidad",
                        "La especialidad seleccionada no existe.");
                }
            }

            string nombreUsuario = (NuevoUsuario.NombreUsuario ?? "").Trim();

            if (nombreUsuario.Length > 0 &&
                await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario))
            {
                ModelState.AddModelError(
                    "NuevoUsuario.NombreUsuario",
                    "Ese nombre de usuario ya está registrado.");
            }

            string nombreCompleto = requierePerfilMedico
                ? $"{NuevoUsuario.NombresMedico.Trim()} {NuevoUsuario.ApellidosMedico.Trim()}".Trim()
                : (NuevoUsuario.NombreCompleto ?? "").Trim();

            if (nombreCompleto.Length > 150)
            {
                ModelState.AddModelError(
                    requierePerfilMedico
                        ? "NuevoUsuario.ApellidosMedico"
                        : "NuevoUsuario.NombreCompleto",
                    "El nombre completo no puede superar 150 caracteres.");
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosAsync();
                return Page();
            }

            await using var transaccion = await _context.Database.BeginTransactionAsync();

            try
            {
                var usuario = new Usuario
                {
                    NombreCompleto = nombreCompleto,
                    FechaNacimiento = NuevoUsuario.FechaNacimiento!.Value.Date,
                    Area = NuevoUsuario.Area.Trim(),
                    Puesto = NuevoUsuario.Puesto.Trim(),
                    CorreoElectronico = NuevoUsuario.CorreoElectronico.Trim(),
                    NombreUsuario = nombreUsuario,
                    Rol = NuevoUsuario.Rol,
                    Estado = true,
                    ContrasenaHasheada = true
                };

                usuario.Contraseña = _passwordHasher.HashPassword(
                    usuario,
                    NuevoUsuario.Contraseña);

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                if (requierePerfilMedico)
                {
                    var medico = new Medico
                    {
                        Nombres = NuevoUsuario.NombresMedico.Trim(),
                        Apellidos = NuevoUsuario.ApellidosMedico.Trim(),
                        Telefono = string.IsNullOrWhiteSpace(NuevoUsuario.Telefono)
                            ? ""
                            : NuevoUsuario.Telefono.Trim(),
                        CorreoElectronico = NuevoUsuario.CorreoElectronico.Trim(),
                        IdUsuario = usuario.IdUsuario,
                        IdEspecialidad = esMedico
                            ? NuevoUsuario.IdEspecialidad
                            : null,
                        Estado = true
                    };

                    _context.Medicos.Add(medico);
                    await _context.SaveChangesAsync();
                }

                await transaccion.CommitAsync();

                TempData["Mensaje"] = requierePerfilMedico
                    ? "La cuenta y el perfil médico se registraron correctamente."
                    : "El usuario se registró correctamente.";

                return RedirectToPage();
            }
            catch (DbUpdateException)
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible guardar el usuario y su perfil. No se conservaron cambios parciales.");

                await CargarDatosAsync();
                return Page();
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        private async Task CargarDatosAsync()
        {
            Usuarios = await _context.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.IdUsuario) //para ordenar por ID
                .ToListAsync();

            Especialidades = await _context.Especialidades
                .AsNoTracking()
                .OrderBy(e => e.Nombre)
                .ToListAsync();
        }
    }
}