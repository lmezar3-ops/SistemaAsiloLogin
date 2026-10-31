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

        // Un médico general puede no tener especialidad.
        public int? IdEspecialidad { get; set; }

        // La cuenta puede permanecer sin vincular
        // mientras se conservan los perfiles anteriores.
        public int? IdUsuario { get; set; }

        public bool Estado { get; set; } = true;

        [ForeignKey(nameof(IdEspecialidad))]
        public Especialidad? Especialidad { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario? Usuario { get; set; }
    }
}