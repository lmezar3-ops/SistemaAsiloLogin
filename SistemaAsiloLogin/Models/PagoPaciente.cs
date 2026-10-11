using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class PagoPaciente
    {
        [Key]
        public int IdPago { get; set; }

        public int? IdCargo { get; set; }

        public int? IdCuota { get; set; }

        public DateTime FechaPago { get; set; }

        public decimal MontoPagado { get; set; }

        public string MetodoPago { get; set; } = "";

        public string Observaciones { get; set; } = "";

        [ForeignKey("IdCargo")]
        public CargoPaciente? CargoPaciente { get; set; }

        [ForeignKey("IdCuota")]
        public CuotaFamiliar? CuotaFamiliar { get; set; }
    }
}