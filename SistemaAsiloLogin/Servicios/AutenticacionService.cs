
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Servicios
{
    public class AutenticacionService
    {
        private readonly AsiloDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public AutenticacionService(AsiloDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        public async Task<Usuario?> IniciarSesion(
            string nombreUsuario,
            string contraseña)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.NombreUsuario == nombreUsuario &&
                    u.Estado);

            if (usuario == null || !usuario.ContrasenaHasheada)
            {
                return null;
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.Contraseña,
                contraseña);

            if (resultado == PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (resultado ==
                PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.Contraseña = _passwordHasher.HashPassword(
                    usuario,
                    contraseña);

                await _context.SaveChangesAsync();
            }

            return usuario;
        }
    }
}