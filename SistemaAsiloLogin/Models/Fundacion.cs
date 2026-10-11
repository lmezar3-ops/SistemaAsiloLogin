using System.ComponentModel.DataAnnotations;

namespace SistemaAsiloLogin.Models
{
    public class Fundacion
    {
        [Key]
        public int IdFundacion { get; set; }

        public string Nombre { get; set; } = "";

        public string Telefono { get; set; } = "";

        public string CorreoElectronico { get; set; } = "";

        public string Direccion { get; set; } = "";

        public bool Estado { get; set; }
    }
}