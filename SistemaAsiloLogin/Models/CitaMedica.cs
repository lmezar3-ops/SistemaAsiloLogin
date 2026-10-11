using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class CitaMedica
    {
        [Key]
        public int IdCita { get; set; }

        public int IdSolicitud { get; set; }

        public int IdMedico { get; set; }

        public DateTime FechaCita { get; set; }

        public string Estado { get; set; } = "Programada";

        public string Observaciones { get; set; } = "";

        [ForeignKey("IdSolicitud")]
        public SolicitudMedica? SolicitudMedica { get; set; }

        [ForeignKey("IdMedico")]
        public Medico? Medico { get; set; }
    }
}