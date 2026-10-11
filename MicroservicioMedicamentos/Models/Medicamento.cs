
using System.ComponentModel.DataAnnotations;

namespace MicroservicioMedicamentos.Models
{
    public class Medicamento
    {
        [Key]
        public int IdMedicamento { get; set; }

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = "";

        [StringLength(300)]
        public string? Descripcion { get; set; }

        public int Existencia { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        public decimal Costo { get; set; }

        public bool Estado { get; set; }
    }
}