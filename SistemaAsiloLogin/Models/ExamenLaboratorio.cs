using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class ExamenLaboratorio
    {
        [Key]
        public int IdExamen { get; set; }

        public int IdVisita { get; set; }

        public string NombreExamen { get; set; } = "";

        public DateTime FechaSolicitud { get; set; }

        public string Estado { get; set; } = "Pendiente";

        public decimal? Costo { get; set; }

        [ForeignKey("IdVisita")]
        public VisitaMedica? VisitaMedica { get; set; }
    }
}