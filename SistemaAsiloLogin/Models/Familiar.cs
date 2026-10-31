
using System.ComponentModel.DataAnnotations;

namespace SistemaAsiloLogin.Models
{
    public class Familiar
    {
        [Key]
        public int IdFamiliar { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        public string Nombres { get; set; } = "";

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        public string Apellidos { get; set; } = "";

        [Required(ErrorMessage = "El parentesco es obligatorio.")]
        public string Parentesco { get; set; } = "";

        public string? Telefono { get; set; }

        public string? CorreoElectronico { get; set; }

        public string? Direccion { get; set; }

        public bool Estado { get; set; }
    }
}
