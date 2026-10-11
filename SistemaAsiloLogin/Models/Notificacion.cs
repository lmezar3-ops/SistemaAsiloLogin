using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class Notificacion
    {
        [Key]
        public int IdNotificacion { get; set; }

        public int IdPaciente { get; set; }

        public int? IdFamiliar { get; set; }

        public DateTime FechaNotificacion { get; set; }

        public string Tipo { get; set; } = "";

        public string Asunto { get; set; } = "";

        public string Mensaje { get; set; } = "";

        public string Estado { get; set; } = "Pendiente";

        [ForeignKey("IdPaciente")]
        public Paciente? Paciente { get; set; }

        [ForeignKey("IdFamiliar")]
        public Familiar? Familiar { get; set; }
    }
}