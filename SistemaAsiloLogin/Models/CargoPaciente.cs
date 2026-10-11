using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class CargoPaciente
    {
        [Key]
        public int IdCargo { get; set; }

        public int IdPaciente { get; set; }

        public int IdFundacion { get; set; }

        public DateTime FechaCargo { get; set; }

        public string TipoCargo { get; set; } = "";

        public string Concepto { get; set; } = "";

        public decimal MontoOriginal { get; set; }

        public decimal Descuento { get; set; }

        public decimal MontoFinal { get; set; }


        public string Estado { get; set; } = "Pendiente";

        [ForeignKey("IdPaciente")]
        public Paciente? Paciente { get; set; }

        [ForeignKey("IdFundacion")]
        public Fundacion? Fundacion { get; set; }
    }
}