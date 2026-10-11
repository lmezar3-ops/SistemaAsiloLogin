using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class Medico
    {
        [Key]
        public int IdMedico { get; set; }

        public string Nombres { get; set; } = "";

        public string Apellidos { get; set; } = "";

        public string Telefono { get; set; } = "";

        public string CorreoElectronico { get; set; } = "";

        public int IdEspecialidad { get; set; }

        public bool Estado { get; set; }

        [ForeignKey("IdEspecialidad")]
        public Especialidad? Especialidad { get; set; }
    }
}