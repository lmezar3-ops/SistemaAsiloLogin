
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    [Table("MedicacionHabitual")]
    public class MedicacionHabitual
    {
        [Key]
        public int IdMedicacionHabitual { get; set; }

        public int IdPaciente { get; set; }

        public int? IdMedicamento { get; set; }

        [StringLength(150)]
        public string? NombreMedicamentoInformado { get; set; }

        [StringLength(100)]
        public string? Dosis { get; set; }

        [StringLength(100)]
        public string? Frecuencia { get; set; }

        [StringLength(500)]
        public string? Indicaciones { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public bool Estado { get; set; } = true;

        public bool RequiereVerificacion { get; set; } = false;
    }
}
