using System.ComponentModel.DataAnnotations;

namespace SistemaAsiloLogin.Models
{
    public class Enfermero
    {
        [Key]
        public int IdEnfermero { get; set; }

        public string Nombres { get; set; } = "";

        public string Apellidos { get; set; } = "";

        public string Telefono { get; set; } = "";

        public bool Estado { get; set; }
    }
}