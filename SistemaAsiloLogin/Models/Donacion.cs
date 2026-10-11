using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class Donacion
    {
        [Key]
        public int IdDonacion { get; set; }

        public int IdDonante { get; set; }

        public DateTime FechaDonacion { get; set; }

        public string TipoDonacion { get; set; } = "";

        public string? Descripcion { get; set; }

        public decimal Monto { get; set; }

        [ForeignKey("IdDonante")]
        public Donante? Donante { get; set; }
    }
}