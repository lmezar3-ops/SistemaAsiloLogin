using System.ComponentModel.DataAnnotations;

namespace SistemaAsiloLogin.Models
{
    public class MovimientoCaja
    {
        [Key]
        public int IdMovimiento { get; set; }

        public DateTime FechaMovimiento { get; set; }

        public string TipoMovimiento { get; set; } = "";

        public string Concepto { get; set; } = "";

        public decimal Monto { get; set; }

        public string? Observaciones { get; set; }

        public string Estado { get; set; } = "Registrado";
    }
}