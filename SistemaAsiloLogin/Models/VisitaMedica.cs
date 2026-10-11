using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class VisitaMedica
    {
        [Key]
        public int IdVisita { get; set; }

        public int IdPaciente { get; set; }

        public int IdMedico { get; set; }

        public int IdCita { get; set; }

        public DateTime FechaVisita { get; set; }

        public string MotivoConsulta { get; set; } = "";

        public string Diagnostico { get; set; } = "";

        public string Observaciones { get; set; } = "";

        [ForeignKey("IdPaciente")]
        public Paciente? Paciente { get; set; }

        [ForeignKey("IdMedico")]
        public Medico? Medico { get; set; }

        [ForeignKey("IdCita")]
        public CitaMedica? CitaMedica { get; set; }
    }
}