using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class Tratamiento
    {
        [Key]
        public int IdTratamiento { get; set; }

        public int IdVisita { get; set; }

        public int IdMedicamento { get; set; }

        public string Dosis { get; set; } = "";

        public string Frecuencia { get; set; } = "";

        public string Duracion { get; set; } = "";

        public int Cantidad { get; set; }

        public string Indicaciones { get; set; } = "";

        [ForeignKey("IdVisita")]
        public VisitaMedica? VisitaMedica { get; set; }
    }
}
