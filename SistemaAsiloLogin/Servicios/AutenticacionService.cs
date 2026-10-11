using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Servicios
{
    public class AutenticacionService
    {
        private readonly AsiloDbContext _context;

        public AutenticacionService(AsiloDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> IniciarSesion(string nombreUsuario, string contraseña)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.NombreUsuario == nombreUsuario &&
                    u.Contraseña == contraseña &&
                    u.Estado);
        }
    }
}