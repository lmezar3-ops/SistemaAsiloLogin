using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroservicioMedicamentos.Models
{
    [Table("EntregaMedicamento")]
    public class EntregaMedicamento
    {
        [Key]
        [Column("IdEntrega")]
        public int IdEntrega { get; set; }

        [Column("IdTratamiento")]
        public int IdTratamiento { get; set; }

        [Column("IdMedicamento")]
        public int IdMedicamento { get; set; }

        [Column("Cantidad")]
        public int Cantidad { get; set; }

        [Column("FechaEntrega")]
        public DateTime FechaEntrega { get; set; }

        [ForeignKey(nameof(IdMedicamento))]
        public Medicamento? Medicamento { get; set; }
    }
}
