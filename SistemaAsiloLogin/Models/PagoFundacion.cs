using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class PagoFundacion
    {
        [Key]
        public int IdPagoFundacion { get; set; }

        public int IdFundacion { get; set; }

        public DateTime FechaPago { get; set; }

        public decimal Monto { get; set; }

        public string? Descripcion { get; set; }

        public string Estado { get; set; } = "Pagado";

        [ForeignKey("IdFundacion")]
        public Fundacion? Fundacion { get; set; }
    }
}
