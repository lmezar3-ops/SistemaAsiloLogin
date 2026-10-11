using System.ComponentModel.DataAnnotations;

namespace SistemaAsiloLogin.Models
{
    public class Gasto
    {
        [Key]
        public int IdGasto { get; set; }

        public DateTime FechaGasto { get; set; }

        public string TipoGasto { get; set; } = "";

        public string Concepto { get; set; } = "";

        public decimal Monto { get; set; }

        public string? Observaciones { get; set; }

        public string Estado { get; set; } = "Registrado";
    }
}