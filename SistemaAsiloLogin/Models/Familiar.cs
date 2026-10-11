using System.ComponentModel.DataAnnotations;

namespace SistemaAsiloLogin.Models
{
    public class Familiar
    {
        [Key]
        public int IdFamiliar { get; set; }

        public string Nombres { get; set; } = "";

        public string Apellidos { get; set; } = "";

        public string Parentesco { get; set; } = "";

        public string Telefono { get; set; } = "";

        public string CorreoElectronico { get; set; } = "";

        public string Direccion { get; set; } = "";

        public bool Estado { get; set; }
    }
}