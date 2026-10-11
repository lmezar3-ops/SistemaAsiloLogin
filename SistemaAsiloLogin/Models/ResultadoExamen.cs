using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class ResultadoExamen
    {
        [Key]
        public int IdResultado { get; set; }

        public int IdExamen { get; set; }

        public string Resultado { get; set; } = "";

        public DateTime FechaResultado { get; set; }

        public string Observaciones { get; set; } = "";

        [ForeignKey("IdExamen")]
        public ExamenLaboratorio? ExamenLaboratorio { get; set; }
    }
}
