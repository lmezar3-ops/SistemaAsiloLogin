using System.ComponentModel.DataAnnotations;

namespace MicroservicioDonaciones.Models
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

        public Donante? Donante { get; set; }
    }
}