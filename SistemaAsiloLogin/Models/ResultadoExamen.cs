
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class ResultadoExamen
    {
        [Key]
        public int IdResultado { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Selecciona un examen válido.")]
        public int IdExamen { get; set; }

        [Required(ErrorMessage = "El resultado es obligatorio.")]
        public string Resultado { get; set; } = "";

        [CustomValidation(typeof(ResultadoExamen),
            nameof(ValidarFechaResultado))]
        public DateTime FechaResultado { get; set; }

        public static ValidationResult? ValidarFechaResultado(
            DateTime fecha,
            ValidationContext context)
        {
            if (fecha < new DateTime(2000, 1, 1) ||
                fecha > new DateTime(2099, 12, 31, 23, 59, 59))
            {
                return new ValidationResult(
                    "Ingresa una fecha de resultado válida.");
            }

            return ValidationResult.Success;
        }

        public string Observaciones { get; set; } = "";

        [ForeignKey("IdExamen")]
        public ExamenLaboratorio? ExamenLaboratorio { get; set; }
    }
}
