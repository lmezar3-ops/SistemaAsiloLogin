using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class CuotaFamiliar
    {
        [Key]
        public int IdCuota { get; set; }

        public int IdPaciente { get; set; }

        public DateTime FechaCuota { get; set; }

        public decimal Monto { get; set; }

        public string Estado { get; set; } = "Pendiente";

        [ForeignKey("IdPaciente")]
        public Paciente? Paciente { get; set; }
    }
}