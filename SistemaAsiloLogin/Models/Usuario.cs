using System.ComponentModel.DataAnnotations;

namespace SistemaAsiloLogin.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        public string NombreUsuario { get; set; } = "";

        public string Contraseña { get; set; } = "";

        public string Rol { get; set; } = "";

        public bool Estado { get; set; }
    }
}